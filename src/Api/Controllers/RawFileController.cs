using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;

[Route("raw-file")]
[ApiController]
public class RawFileController : ControllerBase
{
    private readonly IRawFilesService _fileStorageService;

    public RawFileController(IRawFilesService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendFileToConversion(
        IFormFile file,
        [FromQuery, Required] string fileName,
        [FromQuery] Guid userUuid,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Arquivo não enviado ou está vazio.");
            }

            using var stream = file.OpenReadStream();
            var fileDetails = await _fileStorageService.SaveRawFileAsync(stream, fileName, userUuid, cancellationToken);
            return Ok(fileDetails);
        }
        catch (RawFileServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao salvar arquivo.",
                Detail = e.Message,
            });
        }
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetFile([FromQuery] string fileName, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("Caminho do arquivo não fornecido.");
            }

            var fileDetails = await _fileStorageService.GetRawFileAsync(fileName, cancellationToken);
            return Ok(fileDetails);
        }
        catch (RawFileServiceException e)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao buscar arquivo.",
                Detail = e.Message,
            });
        }
    }

}

