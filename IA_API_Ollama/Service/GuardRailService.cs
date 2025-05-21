namespace IA_API_Ollama.Service
{
    public class GuardRailService
    {
        private readonly ILogger<GuardRailService> _logger;

        public GuardRailService(ILogger<GuardRailService> logger)
        {
            _logger = logger;
        }

        public bool IsResponseSafe(string response)
        {
            // Verificar por palavras proibidas ou padrões
            var blacklist = new[] { "proibido", "inadequado", "inaceitável" };

            foreach (var word in blacklist)
            {
                if (response.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"Resposta bloqueada por conter a palavra: {word}");
                    return false;
                }
            }

            return true;
        }
    }
}
