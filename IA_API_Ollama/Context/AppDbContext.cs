using Microsoft.EntityFrameworkCore;
using IA_API_Ollama.Models;

//Onde vou comunicar com o banco de dados.
namespace IA_API_Ollama.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ChunkEmbedding> ChunkEmbeddings { get; set; }

    }
}