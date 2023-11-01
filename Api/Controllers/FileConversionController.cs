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
    public async Task<IActionResult> SendFileToConversion(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Arquivo não enviado ou está vazio.");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                await _fileStorageService.SaveFileToConvertAsync(stream, file.FileName);
            }

            return Ok("Arquivo recebido e está na fila de conversão.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Erro no servidor: {ex.Message}");
        }
    }
}

