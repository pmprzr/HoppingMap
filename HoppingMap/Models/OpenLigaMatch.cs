namespace HoppingMap.Models;

public class OpenLigaMatch
{
    public int MatchID { get; set; }
    public DateTime MatchDateTime { get; set; }

    public OpenLigaTeam Team1 { get; set; } = new();
    public OpenLigaTeam Team2 { get; set; } = new();
}