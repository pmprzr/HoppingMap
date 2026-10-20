namespace HoppingMap.Models;

public class Stadium
{
    public string TeamName { get; set; } = "";
    public string StadiumName { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string EuroplanUrl { get; set; } = "";
    public string TicketShopUrl { get; set; } = "";
}