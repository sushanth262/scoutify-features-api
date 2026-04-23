using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scoutify.FeaturesApi.Services;

namespace Scoutify.FeaturesApi.Controllers;

[ApiController]
[Route("api/smart-money")]
[Authorize]
public class SmartMoneyController : ControllerBase
{
    private readonly IFeatureDataService _data;

    public SmartMoneyController(IFeatureDataService data) => _data = data;

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string symbol = "AAPL", CancellationToken cancellationToken = default)
    {
        var dto = await _data.GetSmartMoneyAsync(symbol, cancellationToken).ConfigureAwait(false);
        return Ok(dto);
    }
}
