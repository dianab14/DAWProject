using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MicroSocialPlatform.Services
{
    public class AiModerationService : IAiModerationService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AiModerationService> _logger;

        public AiModerationService(IConfiguration config, ILogger<AiModerationService> logger)
        {
            _logger = logger;

            var apiKey = config["OpenAI:ApiKey"]
                ?? throw new Exception("OpenAI ApiKey missing");

            _http = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<AiModerationResult> AnalyzeAsync(string text)
        {
            try
            {
                var systemPrompt = """
You are a content moderation system.
Check whether the text contains inappropriate language
(insults, hate speech, discriminatory language).

Respond ONLY with valid JSON in this format:
{
  "isAppropriate": true|false,
  "confidence": 0.0-1.0
}

Do not add any other text.
""";

                var body = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = text }
                    },
                    temperature = 0.1,
                    max_tokens = 50
                };

                var response = await _http.PostAsync(
                    "chat/completions",
                    new StringContent(
                        JsonSerializer.Serialize(body),
                        Encoding.UTF8,
                        "application/json")
                );

                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI error: {Raw}", raw);
                    return new AiModerationResult { Success = false };
                }

                using var doc = JsonDocument.Parse(raw);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                var result = JsonSerializer.Deserialize<AiModerationResult>(
                    content!,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                result!.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI moderation failed");
                return new AiModerationResult { Success = false };
            }
        }
    }
}
