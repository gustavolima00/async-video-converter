using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;

[Route("webhook-user")]
[ApiController]
public class WebhookUserController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhookUserController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWebhookUser(
        [FromQuery, Required] string webhookUrl,
        CancellationToken cancellationToken)
    {
        var webhookUser = await _webhookService.CreateWebhookUserAsync(webhookUrl, cancellationToken);
        return Ok(webhookUser);
    }

    [HttpGet]
    [Route("{userUuid}")]
    public async Task<IActionResult> GetWebhookUser(
        [FromRoute] Guid userUuid,
        CancellationToken cancellationToken)
    {
        var webhookUser = await _webhookService.GetWebhookUserAsync(userUuid, cancellationToken);
        return Ok(webhookUser);
    }

    [HttpPut]
    [Route("{userUuid}")]
    public async Task<IActionResult> CreateOrUpdateWebhookUser(
        [FromRoute] Guid userUuid,
        [FromQuery, Required] string webhookUrl,
        [FromQuery] IEnumerable<string> events,
        CancellationToken cancellationToken)
    {
        var webhookUser = await _webhookService.CreateOrUpdateWebhookUserAsync(userUuid, webhookUrl, events, cancellationToken);
        return Ok(webhookUser);
    }
}

