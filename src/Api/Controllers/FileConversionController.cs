using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;


[Route("file-conversion")]
[ApiController]
public class FileConversionController : ControllerBase
{
    private readonly IRawFilesService _fileStorageService;

    public FileConversionController(IRawFilesService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("send-file")]
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
}

