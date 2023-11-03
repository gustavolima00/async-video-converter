using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;

[Route("web-videos")]
[ApiController]
public class WebVideoController : ControllerBase
{
    private readonly IWebVideoService _webVideoService;

    public WebVideoController(IWebVideoService webVideoService)
    {
        _webVideoService = webVideoService;
    }

    [HttpGet("")]
    public async Task<IActionResult> ListWebVideos(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _webVideoService.ListWebVideosAsync(cancellationToken);
            return Ok(result);
        }
        catch (WebVideoServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao salvar arquivo.",
                Detail = e.Message,
            });
        }
    }
}

