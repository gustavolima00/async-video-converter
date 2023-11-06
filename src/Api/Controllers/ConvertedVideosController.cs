using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;

[Route("converted-videos")]
[ApiController]
public class ConvertedVideosController : ControllerBase
{
    private readonly IConvertedVideosService _webVideoService;

    public ConvertedVideosController(IConvertedVideosService webVideoService)
    {
        _webVideoService = webVideoService;
    }

    [HttpGet("")]
    public async Task<IActionResult> ListConvertedVideos(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _webVideoService.ListConvertedVideosAsync(cancellationToken);
            return Ok(result);
        }
        catch (ConvertedVideoServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao salvar arquivo.",
                Detail = e.Message,
            });
        }
    }
}

