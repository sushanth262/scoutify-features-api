using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Scoutify.FeaturesApi.Configs;
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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FeatureDataService> _logger;
    private readonly StocksApiOptions _stocksApiOptions;

    public FeatureDataService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<StocksApiOptions> stocksApiOptions,
        ILogger<FeatureDataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _stocksApiOptions = stocksApiOptions.Value;
        _logger = logger;
    }

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

    public async Task<IReadOnlyList<AiInsightCardDto>> GetAiInsightCardsAsync(CancellationToken cancellationToken = default)
    {
        var symbols = (_stocksApiOptions.CardSymbols ?? Array.Empty<string>())
            .Select(s => s.Trim().ToUpperInvariant())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, _stocksApiOptions.MaxCards))
            .ToArray();

        if (symbols.Length == 0)
        {
            return SeedCards;
        }

        var tasks = symbols.Select(symbol => BuildCardAsync(symbol, cancellationToken)).ToArray();
        var cards = await Task.WhenAll(tasks).ConfigureAwait(false);
        var nonNull = cards.Where(c => c is not null).Cast<AiInsightCardDto>().ToList();
        return nonNull.Count > 0 ? nonNull : SeedCards;
    }

    public async Task<AiChatReplyDto> ChatAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        _ = userId;
        var trimmed = message.Trim();
        if (trimmed.Length == 0)
        {
            return new AiChatReplyDto("Ask a question about markets or a ticker.");
        }

        var symbols = ExtractSymbols(trimmed).Take(3).ToArray();
        if (symbols.Length == 0)
        {
            return new AiChatReplyDto("Include one or more ticker symbols like AAPL, MSFT, or TSLA to get worker-backed analysis.");
        }

        if (symbols.Length == 1)
        {
            var insight = await TryGetInsightAsync(symbols[0], cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(insight))
            {
                return new AiChatReplyDto($"Could not retrieve analysis for {symbols[0]} right now. Please try again.");
            }

            return new AiChatReplyDto(insight);
        }

        var compareTasks = symbols.Select(async symbol =>
        {
            var insight = await TryGetInsightAsync(symbol, cancellationToken).ConfigureAwait(false);
            return (symbol, insight);
        });

        var compared = await Task.WhenAll(compareTasks).ConfigureAwait(false);
        var available = compared.Where(x => !string.IsNullOrWhiteSpace(x.insight)).ToArray();
        if (available.Length == 0)
        {
            return new AiChatReplyDto("Could not retrieve analysis for the requested symbols right now. Please try again.");
        }

        var lines = available.Select(x =>
        {
            var body = x.insight!.Length > 420 ? $"{x.insight[..420]}..." : x.insight;
            return $"{x.symbol}: {body}";
        });

        return new AiChatReplyDto($"Comparison for {string.Join(", ", symbols)}\n\n{string.Join("\n\n", lines)}");
    }

    private async Task<AiInsightCardDto?> BuildCardAsync(string symbol, CancellationToken cancellationToken)
    {
        var insight = await TryGetInsightAsync(symbol, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(insight))
        {
            return null;
        }

        var shortened = insight.Length > 240 ? $"{insight[..240]}..." : insight;
        return new AiInsightCardDto(
            $"{symbol} Analysis",
            shortened,
            $"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
    }

    private async Task<string?> TryGetInsightAsync(string symbol, CancellationToken cancellationToken)
    {
        var baseUrl = (_stocksApiOptions.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/stocks/insights")
        {
            Content = JsonContent.Create(new { symbol })
        };

        var bearer = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(bearer) &&
            AuthenticationHeaderValue.TryParse(bearer, out var authHeader))
        {
            request.Headers.Authorization = authHeader;
        }

        try
        {
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stocks API returned {StatusCode} for symbol {Symbol}", (int)response.StatusCode, symbol);
                return null;
            }

            var payload = await response.Content
                .ReadFromJsonAsync<StocksInsightResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return payload?.Insight;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Stocks API unavailable while fetching insight for {Symbol}", symbol);
            return null;
        }
    }

    private static IEnumerable<string> ExtractSymbols(string input)
    {
        return Regex.Matches(input.ToUpperInvariant(), @"\b[A-Z]{1,5}\b")
            .Select(m => m.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class StocksInsightResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public string Insight { get; set; } = string.Empty;
    }
}
