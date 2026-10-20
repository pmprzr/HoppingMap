using System.Text.Json;
using HoppingMap.Models;

namespace HoppingMap.Services;

/// <summary>
/// Default implementation of <see cref="IStadiumResolver"/>: loads stadium data once from
/// Data/stadiums.json on startup and resolves team names against it in memory.
/// Registered as a singleton, since the underlying data never changes at runtime.
/// </summary>
public class StadiumResolver : IStadiumResolver
{
    private readonly Dictionary<string, Stadium> _stadiums;

    /// <summary>
    /// Production constructor: reads and parses Data/stadiums.json from disk.
    /// Used by ASP.NET Core's dependency injection at startup.
    /// </summary>
    public StadiumResolver(IWebHostEnvironment env)
        : this(LoadFromFile(env))
    {
    }

    /// <summary>
    /// Test-friendly constructor: builds the lookup dictionary directly from an
    /// already-parsed list of stadiums, without touching the file system.
    /// Internal, because it's an implementation detail meant for unit tests only,
    /// not a public part of the class's API.
    /// </summary>
    internal StadiumResolver(IEnumerable<Stadium> stadiums)
    {
        _stadiums = stadiums
            .Where(s => !string.IsNullOrWhiteSpace(s.TeamName))
            .GroupBy(s => Normalize(s.TeamName))
            .ToDictionary(g => g.Key, g => g.First());
    }

    private static List<Stadium> LoadFromFile(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "stadiums.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Stadium>>(json) ?? [];
    }

    /// <inheritdoc />
    public Stadium? Resolve(string teamName)
    {
        return _stadiums.GetValueOrDefault(Normalize(teamName));
    }

    private static string Normalize(string name) => name.Trim().ToLowerInvariant();
}