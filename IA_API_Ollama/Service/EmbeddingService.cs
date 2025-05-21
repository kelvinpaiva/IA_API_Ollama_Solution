using IA_API_Ollama.Context;
using System.Text.Json;
using IA_API_Ollama.Models;

namespace IA_API_Ollama.Service
{
    //Onde vou comunicar com a IA.
    public class EmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;

        public EmbeddingService(AppDbContext db)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
            _db = db;
        }

        public async Task<List<float>> GetEmbeddingAsync(string input, string model = "llama3")
        {
            var request = new { model, prompt = input }; // Ollama usa "prompt" no endpoint de embeddings

            var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
            return result?.Embedding ?? throw new Exception("Failed to get embedding");
        }

        public async Task SaveChunksWithEmbeddingsAsync(List<string> chunks)
        {
            foreach (var chunk in chunks)
            {
                var embedding = await GetEmbeddingAsync(chunk);
                var entity = new ChunkEmbedding
                {
                    Content = chunk,
                    EmbeddingJson = JsonSerializer.Serialize(embedding)
                };

                _db.ChunkEmbeddings.Add(entity);
            }

            await _db.SaveChangesAsync();
        }
    }
}
