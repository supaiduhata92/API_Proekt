using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace API_Proekt.Services
{
    public class GeminiService
    {
        // Hardcoded per your request (not recommended for production)
        private const string ApiKey = "AIzaSyClfhIHsPGlmiISf5wcn-xQNsx2p57M6Lk";
        // You requested "Genma 3 4B" — using a reasonable model id form
        private const string ModelName = "models/gemma-3-4b-it";

        private readonly IHttpClientFactory _httpFactory;

        public GeminiService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<string> GenerateTextAsync(string prompt, int maxOutputTokens = 256, double temperature = 0.2, CancellationToken ct = default)
        {
            var client = _httpFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/{ModelName}:generateContent?key={ApiKey}";

            var body = new
            {
                contents = new[]
                {
            new { parts = new[] { new { text = prompt } } }
        },
                generationConfig = new
                {
                    maxOutputTokens = maxOutputTokens,
                    temperature = temperature
                }
            };

            var json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await client.PostAsync(url, content, ct);

            var respText = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini request failed ({(int)resp.StatusCode}): {respText}");

            try
            {
                using var doc = JsonDocument.Parse(respText);
                var root = doc.RootElement;

                // Try common shapes, per Google responses:
                // candidates[0].content.parts[0].text
                if (root.TryGetProperty("candidates", out var cand) && cand.ValueKind == JsonValueKind.Array && cand.GetArrayLength() > 0)
                {
                    var first = cand[0];
                    if (first.TryGetProperty("content", out var contentEl) && contentEl.ValueKind == JsonValueKind.Object)
                    {
                        if (contentEl.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                        {
                            var part0 = parts[0];
                            if (part0.ValueKind == JsonValueKind.Object && part0.TryGetProperty("text", out var textEl))
                                return textEl.GetString() ?? string.Empty;
                            if (part0.ValueKind == JsonValueKind.String)
                                return part0.GetString() ?? string.Empty;
                        }
                    }
                }

                // fallback: try `candidates[0].content` as plain string
                if (root.TryGetProperty("candidates", out var cand2) && cand2.ValueKind == JsonValueKind.Array && cand2.GetArrayLength() > 0)
                {
                    var first = cand2[0];
                    if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("content", out var contentEl))
                    {
                        return contentEl.ToString();
                    }
                }

                // As final fallback return raw response
                return respText;
            }
            catch
            {
                // parsing failed — return raw response
                return respText;
            }
        }
    }
}