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

    [HttpPost("create")]
    public async Task<IActionResult> CreateWebhookUser(
        [FromQuery, Required] string webhookUrl,
        CancellationToken cancellationToken)
    {
        var webhookUser = await _webhookService.CreateWebhookUserAsync(webhookUrl, cancellationToken);
        return Ok(webhookUser);
    }
}

