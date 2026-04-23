using System.Collections.Concurrent;
using System.Linq;
using Scoutify.FeaturesApi.Models;

namespace Scoutify.FeaturesApi.Services;

/// <summary>
/// Sample async data layer. Swap for databases or downstream APIs in production.
/// </summary>
public sealed class FeatureDataService : IFeatureDataService
{
    private static readonly IReadOnlyList<ScreenerRowDto> SeedScreener =
    [
        new("MSFT", "Microsoft Corporation", "$378.85", "28.5", "$2.8T", "Technology"),
        new("NVDA", "NVIDIA Corporation", "$875.28", "65.2", "$2.1T", "Technology"),
        new("JNJ", "Johnson & Johnson", "$160.45", "15.8", "$420B", "Healthcare")
    ];

    private static readonly IReadOnlyList<FinancialRowDto> SeedFinancial =
    [
        new("AAPL", "Apple Inc.", "$394.3B", "$99.8B", "$6.16", "28.4", "$2.8T", "Technology"),
        new("MSFT", "Microsoft Corporation", "$211.9B", "$72.4B", "$9.65", "28.5", "$2.8T", "Technology"),
        new("GOOGL", "Alphabet Inc.", "$282.8B", "$73.8B", "$5.80", "24.5", "$1.8T", "Technology")
    ];

    private static readonly SmartMoneyDto SeedSmartMoney = new(
        Institutional:
        [
            new("Vanguard Group", "1.28B", "$224.6B", "+12.5M", "+0.99%", "text-green-400"),
            new("BlackRock", "1.02B", "$178.9B", "+8.2M", "+0.81%", "text-green-400"),
            new("Berkshire Hathaway", "915.6M", "$160.5B", "-2.1M", "-0.23%", "text-red-400"),
            new("State Street", "634.8M", "$111.3B", "+4.7M", "+0.75%", "text-green-400")
        ],
        Insider:
        [
            new("Tim Cook", "CEO", "Sale", "511,000", "$175.23", "2024-01-15", "text-red-400"),
            new("Luca Maestri", "CFO", "Sale", "95,000", "$172.45", "2024-01-12", "text-red-400")
        ]);

    private static readonly IReadOnlyList<AiInsightCardDto> SeedCards =
    [
        new("TSLA Analysis",
            "Technical indicators suggest a potential breakout above resistance at $250. Volume patterns support bullish momentum...",
            "Generated 30 minutes ago"),
        new("Market Sentiment",
            "Overall market sentiment remains cautiously optimistic with strong tech sector performance driving indices higher...",
            "Generated 1 hour ago"),
        new("Sector Rotation",
            "Institutional money is flowing from growth to value stocks, suggesting a potential sector rotation in the coming weeks...",
            "Generated 2 hours ago")
    ];

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WatchlistItemDto>> _watchlists = new();

    public Task<IReadOnlyList<WatchlistItemDto>> GetWatchlistAsync(string userId, CancellationToken cancellationToken = default)
    {
        var map = _watchlists.GetOrAdd(userId, _ => new ConcurrentDictionary<string, WatchlistItemDto>(StringComparer.OrdinalIgnoreCase));
        if (map.IsEmpty)
        {
            map["AAPL"] = new WatchlistItemDto("AAPL", "Apple Inc.", "$175.23", "+2.45", "+1.42%", "text-green-400");
            map["GOOGL"] = new WatchlistItemDto("GOOGL", "Alphabet Inc.", "$141.89", "-1.23", "-0.86%", "text-red-400");
            map["MSFT"] = new WatchlistItemDto("MSFT", "Microsoft Corporation", "$378.85", "+5.12", "+1.37%", "text-green-400");
            map["TSLA"] = new WatchlistItemDto("TSLA", "Tesla Inc.", "$234.56", "+8.90", "+3.94%", "text-green-400");
        }

        IReadOnlyList<WatchlistItemDto> list = map.Values.OrderBy(v => v.Symbol).ToList();
        return Task.FromResult(list);
    }

    public Task<WatchlistItemDto> AddWatchlistAsync(string userId, string symbol, CancellationToken cancellationToken = default)
    {
        var sym = symbol.Trim().ToUpperInvariant();
        var map = _watchlists.GetOrAdd(userId, _ => new ConcurrentDictionary<string, WatchlistItemDto>(StringComparer.OrdinalIgnoreCase));
        var row = new WatchlistItemDto(sym, "Company Name", "$0.00", "+0.00", "+0.00%", "text-green-400");
        map[sym] = row;
        return Task.FromResult(row);
    }

    public Task RemoveWatchlistAsync(string userId, string symbol, CancellationToken cancellationToken = default)
    {
        if (_watchlists.TryGetValue(userId, out var map))
        {
            map.TryRemove(symbol.ToUpperInvariant(), out _);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ScreenerRowDto>> RunScreenerAsync(ScreenerFilterDto filters, CancellationToken cancellationToken = default)
    {
        IEnumerable<ScreenerRowDto> q = SeedScreener;
        if (!string.IsNullOrWhiteSpace(filters.Sector) && !filters.Sector.Equals("All Sectors", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(r => r.Sector.Equals(filters.Sector, StringComparison.OrdinalIgnoreCase));
        }

        if (int.TryParse(filters.PeRatioMax, out var maxPe))
        {
            q = q.Where(r => decimal.TryParse(r.Pe, out var pe) && pe <= maxPe);
        }

        return Task.FromResult((IReadOnlyList<ScreenerRowDto>)q.ToList());
    }

    public Task<SmartMoneyDto> GetSmartMoneyAsync(string symbol, CancellationToken cancellationToken = default)
    {
        _ = symbol;
        return Task.FromResult(SeedSmartMoney);
    }

    public Task<IReadOnlyList<FinancialRowDto>> GetMarketDataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(SeedFinancial);

    public Task<IReadOnlyList<AiInsightCardDto>> GetAiInsightCardsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(SeedCards);

    public async Task<AiChatReplyDto> ChatAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        _ = userId;
        await Task.Yield();
        var trimmed = message.Trim();
        if (trimmed.Length == 0)
        {
            return new AiChatReplyDto("Ask a question about markets or a ticker.");
        }

        return new AiChatReplyDto(
            $"You asked: \"{trimmed}\". This desktop stack routes heavy analysis through the .NET worker and message bus; wire your LLM gateway here for full parity.");
    }
}
