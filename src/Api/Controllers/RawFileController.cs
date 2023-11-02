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
    public async Task<IActionResult> SendFileToConversion(IFormFile file, [FromQuery] string fileName, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Arquivo não enviado ou está vazio.");
        }

        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("Nome do arquivo não fornecido.");
        }


        using var stream = file.OpenReadStream();
        var fileDetails = await _fileStorageService.SaveRawFileAsync(stream, fileName, cancellationToken);
        return Ok(fileDetails);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetFile([FromQuery] string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BadRequest("Caminho do arquivo não fornecido.");
        }

        var fileDetails = await _fileStorageService.GetRawFileAsync(path, cancellationToken);
        return Ok(fileDetails);
    }

}

