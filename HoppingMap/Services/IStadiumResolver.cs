using HoppingMap.Models;

namespace HoppingMap.Services;

/// <summary>
/// Looks up stadium information (name, coordinates, external links) for a given team name.
/// Exists as an interface so consumers (e.g. FootballService) depend on "some way to resolve
/// a stadium" rather than on the concrete JSON-file-based implementation.
/// </summary>
public interface IStadiumResolver
{
    /// <summary>
    /// Attempts to find stadium data for the given team name.
    /// </summary>
    /// <param name="teamName">Team name as returned by OpenLigaDB (e.g. "Borussia Dortmund").</param>
    /// <returns>The matching <see cref="Stadium"/>, or null if the team isn't known.</returns>
    Stadium? Resolve(string teamName);
}