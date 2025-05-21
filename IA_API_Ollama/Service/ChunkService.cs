namespace IA_API_Ollama.Service
{
    //Onde vou trabalhar no texto, cortes e etc.
    public class ChunkService
    {
        public List<string> CreateChunks(string text, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<string>();

            var words = text.Split(' ');
            int start = 0;

            while (start < words.Length)
            {
                int end = Math.Min(start + chunkSize, words.Length);
                var chunk = string.Join(' ', words[start..end]);
                chunks.Add(chunk);

                if (end == words.Length) break;
                start = end - overlap;
            }

            return chunks;
        }
    }
}
