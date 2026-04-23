using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scoutify.FeaturesApi.Models;
using Scoutify.FeaturesApi.Services;

namespace Scoutify.FeaturesApi.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiInsightsController : ControllerBase
{
    private readonly IFeatureDataService _data;

    public AiInsightsController(IFeatureDataService data) => _data = data;

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Missing subject.");

    [HttpGet("cards")]
    public async Task<IActionResult> CardsAsync(CancellationToken cancellationToken)
    {
        var cards = await _data.GetAiInsightCardsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new { insights = cards });
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ChatAsync([FromBody] AiChatRequestDto body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Message))
        {
            return BadRequest(new { error = "Message is required" });
        }

        var reply = await _data.ChatAsync(UserId, body.Message, cancellationToken).ConfigureAwait(false);
        return Ok(reply);
    }
}
