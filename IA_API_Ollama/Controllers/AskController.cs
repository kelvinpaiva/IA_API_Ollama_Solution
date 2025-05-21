using IA_API_Ollama.Context;
using IA_API_Ollama.Service;
using IA_API_Ollama.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IA_API_Ollama.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AskController : ControllerBase
    {
        private readonly EmbeddingService _embeddingService;
        private readonly AppDbContext _db;
        private readonly GuardRailService _guardRail;
        private readonly ILogger<AskController> _logger;

        public AskController(EmbeddingService embeddingService, AppDbContext db, GuardRailService guardRail, ILogger<AskController> logger)
        {
            _embeddingService = embeddingService;
            _db = db;
            _guardRail = guardRail;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                _logger.LogWarning("Pergunta vazia recebida.");
                return BadRequest("Pergunta não pode ser vazia.");
            }

            try
            {
                _logger.LogInformation("Calculando embedding da pergunta...");
                var questionEmbedding = await _embeddingService.GetEmbeddingAsync(request.Question);

                _logger.LogInformation("Buscando chunks mais semelhantes...");
                var chunks = _db.ChunkEmbeddings.ToList();
                var scoredChunks = chunks.Select(c => new
                {
                    Chunk = c,
                    Score = CosineSimilarity(questionEmbedding, JsonSerializer.Deserialize<List<float>>(c.EmbeddingJson)!)
                })
                .OrderByDescending(s => s.Score)
                .Take(3)
                .Select(s => s.Chunk.Content)
                .ToList();

                var prompt = $"Contexto:\n{string.Join("\n\n", scoredChunks)}\n\nPergunta: {request.Question}\nResposta:";

                _logger.LogInformation("Enviando prompt para o Ollama...");
                var response = await CallOllamaAsync(prompt);

                if (!_guardRail.IsResponseSafe(response))
                {
                    return BadRequest("A resposta gerada foi considerada inadequada pelos filtros de segurança.");
                }

                return Ok(new { Answer = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar a pergunta.");
                return StatusCode(500, "Erro interno no servidor.");
            }
        }

        private static double CosineSimilarity(List<float> vec1, List<float> vec2)
        {
            double dot = 0.0, normA = 0.0, normB = 0.0;
            for (int i = 0; i < vec1.Count; i++)
            {
                dot += vec1[i] * vec2[i];
                normA += Math.Pow(vec1[i], 2);
                normB += Math.Pow(vec2[i], 2);
            }
            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        private async Task<string> CallOllamaAsync(string prompt, string model = "llama3")
        {
            using var http = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };

            var request = new { model, prompt, stream = false };

            var response = await http.PostAsJsonAsync("/api/generate", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return json?.Response ?? "Sem resposta.";
        }
    }
}
