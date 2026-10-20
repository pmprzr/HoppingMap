using HoppingMap.Services;
using Microsoft.AspNetCore.Mvc;

namespace HoppingMap.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly FootballService _footballService;
    private readonly IConfiguration _configuration;

    public MatchesController(FootballService footballService, IConfiguration configuration)
    {
        _footballService = footballService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var season = _configuration.GetValue<int>("HoppingMap:Season");
        var leagues = _configuration.GetSection("HoppingMap:Leagues").Get<string[]>() ?? [];

        var tasks = leagues.Select(league => _footballService.GetMatchesAsync(league, season));
        var resultsPerLeague = await Task.WhenAll(tasks);

        var allMatches = resultsPerLeague.SelectMany(matches => matches).ToList();

        return Ok(allMatches);
    }
}