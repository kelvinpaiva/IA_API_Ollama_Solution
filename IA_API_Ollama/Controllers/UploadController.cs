using IA_API_Ollama.Service;
using IA_API_Ollama.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IA_API_Ollama.Models;

namespace IA_API_Ollama.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ChunkService _chunkService;
        private readonly EmbeddingService _embeddingService;

        public UploadController(ChunkService chunkService, EmbeddingService embeddingService)
        {
            _chunkService = chunkService;
            _embeddingService = embeddingService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadText([FromBody] UploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Texto não pode ser vazio.");

            var chunks = _chunkService.CreateChunks(request.Text);
            await _embeddingService.SaveChunksWithEmbeddingsAsync(chunks);

            return Ok(new { Message = "Documento processado com sucesso", TotalChunks = chunks.Count });
        }
    }
}
