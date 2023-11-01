using Microsoft.AspNetCore.Mvc;
using Services;

namespace Api.Controllers;


[Route("file-conversion")]
[ApiController]
public class FileConversionController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FileConversionController(IFileStorageService fileStorageService)
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

        try
        {
            using var stream = file.OpenReadStream();
            var fileDetails = await _fileStorageService.SaveFileToConvertAsync(stream, fileName, cancellationToken);
            return Ok(fileDetails);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Erro no servidor: {ex.Message}");
        }
    }
}

