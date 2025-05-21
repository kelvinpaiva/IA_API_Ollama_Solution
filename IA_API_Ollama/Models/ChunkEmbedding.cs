using System.ComponentModel.DataAnnotations;

namespace IA_API_Ollama.Models
{
    public class ChunkEmbedding
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string EmbeddingJson { get; set; } = string.Empty; // Salva o vetor como JSON
    }
}
