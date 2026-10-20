using System.Net.Http.Json;
using HoppingMap.Models;

namespace HoppingMap.Services;

/// <summary>
/// Fetches match data from the OpenLigaDB API and enriches it with stadium
/// information (name, coordinates, external links) resolved via <see cref="IStadiumResolver"/>.
/// </summary>
public class FootballService
{
    private readonly HttpClient _httpClient;
    private readonly IStadiumResolver _stadiumResolver;

    public FootballService(HttpClient httpClient, IStadiumResolver stadiumResolver)
    {
        _httpClient = httpClient;
        _stadiumResolver = stadiumResolver;
    }

    /// <summary>
    /// Determines the current matchday (group) for a given league, as reported by
    /// OpenLigaDB's "getcurrentgroup" endpoint. Falls back to matchday 1 if the
    /// API returns no group (e.g. unexpected/empty response).
    /// </summary>
    /// <param name="league">OpenLigaDB league shortcut, e.g. "bl1", "bl2", "bl3".</param>
    /// <returns>The current matchday number (GroupOrderID).</returns>
    // Get all games of the current matchday (all upcoming matches)
    // TODO260911: check how this function behaves on postponed matches
    public async Task<int> GetCurrentMatchdayAsync(string league)
    {
        var url = $"https://api.openligadb.de/getcurrentgroup/{league}";
        var group = await _httpClient.GetFromJsonAsync<OpenLigaGroup>(url);
        return group?.GroupOrderID ?? 1;
    }

    /// <summary>
    /// Fetches all matches of the current matchday for a given league and season,
    /// and enriches each match with its home team's stadium data (name, coordinates,
    /// Europlan link, ticket shop link) via <see cref="IStadiumResolver"/>.
    /// Stadium fields are left empty/zero when the home team isn't found in stadiums.json.
    /// </summary>
    /// <param name="league">OpenLigaDB league shortcut, e.g. "bl1", "bl2", "bl3".</param>
    /// <param name="season">Season start year, e.g. 2026 for the 2026/27 season.</param>
    /// <returns>All matches of the current matchday, or an empty list if the API returns none.</returns>
    // Get particular match
    public async Task<List<Match>> GetMatchesAsync(string league, int season)
    {
        var matchday = await GetCurrentMatchdayAsync(league);

        var url = $"https://api.openligadb.de/getmatchdata/{league}/{season}/{matchday}";
        var matches = await _httpClient.GetFromJsonAsync<List<OpenLigaMatch>>(url);

        if (matches == null)
            return [];

        return matches
            .Select(x =>
            {
                var stadium = _stadiumResolver.Resolve(x.Team1.TeamName);

                return new Match
                {
                    League = league,
                    HomeTeam = x.Team1.TeamName,
                    AwayTeam = x.Team2.TeamName,
                    StartTime = x.MatchDateTime,
                    Stadium = stadium?.StadiumName ?? "",
                    Latitude = stadium?.Latitude ?? 0,
                    Longitude = stadium?.Longitude ?? 0,
                    EuroplanUrl = stadium?.EuroplanUrl ?? "",
                    TicketShopUrl = stadium?.TicketShopUrl ?? ""
                };
            })
            .ToList();
    }
}