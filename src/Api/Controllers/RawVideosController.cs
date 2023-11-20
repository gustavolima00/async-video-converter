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
    public async Task<IActionResult> SendSubtitleAsync(
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

    [HttpGet("{rawVideoUuid}")]
    public async Task<IActionResult> GetRawVideo(
        [FromRoute, Required] Guid rawVideoUuid,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var fileDetails = await _rawVideosService.GetAsync(rawVideoUuid, cancellationToken);
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

    [HttpGet]
    public async Task<IActionResult> ListUserRawVideos(
        [FromQuery, Required] Guid userUuid,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var rawVideos = await _rawVideosService.GetByUserUuidAsync(userUuid, cancellationToken);
            return Ok(rawVideos);
        }
        catch (ServicesException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao buscar arquivos.",
                Detail = e.Message,
            });
        }
    }
}

