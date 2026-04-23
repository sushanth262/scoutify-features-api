using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scoutify.FeaturesApi.Models;
using Scoutify.FeaturesApi.Services;

namespace Scoutify.FeaturesApi.Controllers;

[ApiController]
[Route("api/screener")]
[Authorize]
public class ScreenerController : ControllerBase
{
    private readonly IFeatureDataService _data;

    public ScreenerController(IFeatureDataService data) => _data = data;

    [HttpPost("run")]
    public async Task<IActionResult> RunAsync([FromBody] ScreenerFilterDto filters, CancellationToken cancellationToken)
    {
        _ = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var rows = await _data.RunScreenerAsync(filters, cancellationToken).ConfigureAwait(false);
        return Ok(new { results = rows });
    }
}
