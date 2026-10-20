using HoppingMap.Models;
using HoppingMap.Services;
using Xunit;

namespace HoppingMap.Tests;

public class StadiumResolverTests
{
    // Verifies that the resolver returns the correct stadium when the team name matches exactly.
    [Fact]
    public void Resolve_ReturnsStadium_WhenTeamNameMatchesExactly()
    {
        var stadiums = new List<Stadium>
        {
            new() { TeamName = "Borussia Dortmund", StadiumName = "Signal Iduna Park" }
        };
        var resolver = new StadiumResolver(stadiums);

        var result = resolver.Resolve("Borussia Dortmund");

        Assert.NotNull(result);
        Assert.Equal("Signal Iduna Park", result!.StadiumName);
    }

    // Verifies that the resolver ignores casing and leading or trailing whitespace when searching for a team.
    [Theory]
    [InlineData("borussia dortmund")]
    [InlineData("BORUSSIA DORTMUND")]
    [InlineData("  Borussia Dortmund  ")]
    public void Resolve_IgnoresCasingAndWhitespace(string lookupName)
    {
        var stadiums = new List<Stadium>
        {
            new() { TeamName = "Borussia Dortmund", StadiumName = "Signal Iduna Park" }
        };
        var resolver = new StadiumResolver(stadiums);

        var result = resolver.Resolve(lookupName);

        Assert.NotNull(result);
    }

    // Verifies that the resolver returns null when the requested team is not known.
    [Fact]
    public void Resolve_ReturnsNull_WhenTeamNotKnown()
    {
        var resolver = new StadiumResolver(new List<Stadium>());

        var result = resolver.Resolve("Unknown FC");

        Assert.Null(result);
    }

    // Verifies that entries with blank or whitespace-only team names are ignored by the constructor.
    [Fact]
    public void Constructor_SkipsEntriesWithBlankTeamName()
    {
        var stadiums = new List<Stadium>
        {
            new() { TeamName = "", StadiumName = "Sollte ignoriert werden" },
            new() { TeamName = "   ", StadiumName = "Auch ignoriert" },
            new() { TeamName = "Real Team", StadiumName = "Echtes Stadion" }
        };
        var resolver = new StadiumResolver(stadiums);

        Assert.Null(resolver.Resolve(""));
        Assert.NotNull(resolver.Resolve("Real Team"));
    }

    // Verifies that the first entry is kept when the same team name appears multiple times.
    [Fact]
    public void Constructor_KeepsFirstEntry_WhenTeamNameAppearsTwice()
    {
        var stadiums = new List<Stadium>
        {
            new() { TeamName = "1. FC Union Berlin", StadiumName = "Erstes (richtiges) Stadion" },
            new() { TeamName = "1. FC Union Berlin", StadiumName = "Zweites (falsches) Stadion" }
        };
        var resolver = new StadiumResolver(stadiums);

        var result = resolver.Resolve("1. FC Union Berlin");

        Assert.Equal("Erstes (richtiges) Stadion", result!.StadiumName);
    }
}