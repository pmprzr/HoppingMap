namespace HoppingMap.Models;

public class Match
{
    public string League { get; set; } = "";
    public string HomeTeam { get; set; } = "";
    public string AwayTeam { get; set; } = "";
    public DateTime StartTime { get; set; }
    public string Stadium { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string EuroplanUrl { get; set; } = "";
    public string TicketShopUrl { get; set; } = "";
}