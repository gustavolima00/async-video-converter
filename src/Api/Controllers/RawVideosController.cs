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
    private readonly IVideoConversionService _videoConversionService;

    public RawVideosController(
        IRawVideoService rawVideosService,
        IVideoConversionService videoConversionService
    )
    {
        _rawVideosService = rawVideosService;
        _videoConversionService = videoConversionService;
    }

    [HttpPut("send-video")]
    public async Task<IActionResult> SendVideoAsync(
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
            var fileExtension = Path.GetExtension(file.FileName) ?? throw new RawVideoServiceException("File extension not found");
            await _videoConversionService.SaveSubtitleAsync(userUuid, stream, fileExtension, language, rawVideoName, cancellationToken);
            return Ok();
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

