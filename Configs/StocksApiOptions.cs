namespace Scoutify.FeaturesApi.Configs;

public class StocksApiOptions
{
    public string BaseUrl { get; set; } = "http://scoutify-stocks-api:8080";
    public int MaxCards { get; set; } = 3;
    public string[] CardSymbols { get; set; } = ["AAPL", "MSFT", "TSLA"];
}
