using Scoutify.FeaturesApi.Models;

namespace Scoutify.FeaturesApi.Services;

public interface IFeatureDataService
{
    Task<IReadOnlyList<WatchlistItemDto>> GetWatchlistAsync(string userId, CancellationToken cancellationToken = default);
    Task<WatchlistItemDto> AddWatchlistAsync(string userId, string symbol, CancellationToken cancellationToken = default);
    Task RemoveWatchlistAsync(string userId, string symbol, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScreenerRowDto>> RunScreenerAsync(ScreenerFilterDto filters, CancellationToken cancellationToken = default);
    Task<SmartMoneyDto> GetSmartMoneyAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FinancialRowDto>> GetMarketDataAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiInsightCardDto>> GetAiInsightCardsAsync(CancellationToken cancellationToken = default);
    Task<AiChatReplyDto> ChatAsync(string userId, string message, CancellationToken cancellationToken = default);
}
