using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scoutify.FeaturesApi.Services;

namespace Scoutify.FeaturesApi.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public class WatchlistController : ControllerBase
{
    private readonly IFeatureDataService _data;

    public WatchlistController(IFeatureDataService data) => _data = data;

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Missing subject.");

    [HttpGet("items")]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var items = await _data.GetWatchlistAsync(UserId, cancellationToken).ConfigureAwait(false);
        return Ok(new { items });
    }

    public record AddBody(string Symbol);

    [HttpPost("items")]
    public async Task<IActionResult> AddAsync([FromBody] AddBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Symbol))
        {
            return BadRequest(new { error = "Symbol is required" });
        }

        var item = await _data.AddWatchlistAsync(UserId, body.Symbol, cancellationToken).ConfigureAwait(false);
        return Ok(item);
    }

    [HttpDelete("items/{symbol}")]
    public async Task<IActionResult> RemoveAsync(string symbol, CancellationToken cancellationToken)
    {
        await _data.RemoveWatchlistAsync(UserId, symbol, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
