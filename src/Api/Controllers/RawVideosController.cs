using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Exceptions;

namespace Api.Controllers;

[Route("raw-videos")]
[ApiController]
public class RawVideosController : ControllerBase
{
    private readonly IRawVideoService _rawVideosService;
    private readonly IRawSubtitlesService _rawSubtitlesService;

    public RawVideosController(
        IRawVideoService rawVideosService,
        IRawSubtitlesService rawSubtitlesService
    )
    {
        _rawVideosService = rawVideosService;
        _rawSubtitlesService = rawSubtitlesService;
    }

    [HttpPut("send-video")]
    public async Task<IActionResult> SendVideoToConversion(
        [Required] IFormFile file,
        [FromQuery, Required] string fileName,
        [FromQuery, Required] Guid userUuid,
        CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var rawVideo = await _rawVideosService.SaveAsync(userUuid, stream, fileName, cancellationToken);
            return Ok(rawVideo);
        }
        catch (ServicesException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao salvar vídeo.",
                Detail = e.Message,
            });
        }
    }

    [HttpPut("send-subtitle")]
    public async Task<IActionResult> SendSubtitleToConversion(
        [Required] IFormFile file,
        [FromQuery, Required] string language,
        [FromQuery, Required] string rawVideoName,
        [FromQuery, Required] Guid userUuid,
        CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var rawVideo = await _rawSubtitlesService.SaveAsync(userUuid, stream, language, rawVideoName, cancellationToken);
            return Ok(rawVideo);
        }
        catch (RawVideoServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao salvar legenda.",
                Detail = e.Message,
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFile(
        [FromQuery, Required] string fileName,
        [FromQuery, Required] Guid userUuid,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var fileDetails = await _rawVideosService.GetAsync(userUuid, fileName, cancellationToken);
            return Ok(fileDetails);
        }
        catch (ServicesException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao buscar arquivo.",
                Detail = e.Message,
            });
        }
    }
}

