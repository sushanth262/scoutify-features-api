namespace Scoutify.FeaturesApi.Models;

public record WatchlistItemDto(
    string Symbol,
    string Company,
    string Price,
    string Change,
    string ChangePercent,
    string ChangeColor);

public record ScreenerFilterDto(string? MarketCap, string? PeRatioMax, string? Sector);

public record ScreenerRowDto(string Symbol, string Company, string Price, string Pe, string MarketCap, string Sector);

public record InstitutionalHoldingDto(
    string Institution,
    string Shares,
    string Value,
    string Change,
    string ChangePercent,
    string ChangeColor);

public record InsiderTransactionDto(
    string Insider,
    string Position,
    string TransactionType,
    string Shares,
    string Price,
    string Date,
    string TypeColor);

public record SmartMoneyDto(
    IReadOnlyList<InstitutionalHoldingDto> Institutional,
    IReadOnlyList<InsiderTransactionDto> Insider);

public record FinancialRowDto(
    string Symbol,
    string Company,
    string Revenue,
    string NetIncome,
    string Eps,
    string Pe,
    string MarketCap,
    string Sector);

public record AiInsightCardDto(string Title, string Content, string Timestamp);

public record AiChatRequestDto(string Message);

public record AiChatReplyDto(string Reply);
