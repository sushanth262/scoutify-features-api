using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scoutify.FeaturesApi.Services;

namespace Scoutify.FeaturesApi.Controllers;

[ApiController]
[Route("api/market-data")]
[Authorize]
public class MarketDataController : ControllerBase
{
    private readonly IFeatureDataService _data;

    public MarketDataController(IFeatureDataService data) => _data = data;

    [HttpGet("financials")]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var rows = await _data.GetMarketDataAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new { rows });
    }
}
