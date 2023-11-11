using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;

[Route("raw-videos")]
[ApiController]
public class RawVideosController : ControllerBase
{
    private readonly IRawVideoService _rawFileService;

    public RawVideosController(IRawVideoService fileStorageService)
    {
        _rawFileService = fileStorageService;
    }

    [HttpPost("send-video")]
    public async Task<IActionResult> SendVideoToConversion(
        [Required] IFormFile file,
        [FromQuery, Required] string fileName,
        [FromQuery, Required] Guid userUuid,
        CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var fileDetails = await _rawFileService.SaveRawVideoAsync(userUuid, stream, fileName, cancellationToken);
            return Ok(fileDetails);
        }
        catch (RawVideoServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao salvar vídeo.",
                Detail = e.Message,
            });
        }
    }

    [HttpPost("send-subtitle")]
    public async Task<IActionResult> SendSubtitleToConversion(
        [Required] IFormFile file,
        [FromQuery, Required] string fileName,
        [FromQuery, Required] string rawVideoName,
        [FromQuery, Required] Guid userUuid,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Arquivo não enviado ou está vazio.");
            }

            using var stream = file.OpenReadStream();
            var rawVideo = await _rawFileService.SaveRawSubtitleAsync(userUuid, stream, fileName, rawVideoName, cancellationToken);
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
            var fileDetails = await _rawFileService.GetRawVideoAsync(userUuid, fileName, cancellationToken);
            return Ok(fileDetails);
        }
        catch (RawVideoServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao buscar arquivo.",
                Detail = e.Message,
            });
        }
    }
}

