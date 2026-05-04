using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EventGenie.Services
{
    public class ChatgptServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public ChatgptServices(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured in appsettings.json");
            _model = configuration["OpenAI:Model"] ?? "gpt-4";
        }

        public async Task<string> GetSuggestionsAsync(string prompt)
        {
            var requestContent = new
            {
                model = _model,
                messages = new[]
                {
                    new {
                        role = "system",
                        content = @"You are a JSON-only API. You MUST respond with ONLY a raw JSON object — no explanations, no markdown, no code fences, no text before or after.

RULES:
1. Select exactly 3 events from the list. If fewer than 3 exist, select all.
2. Each selected event must have a different RequestGroupId when possible.
3. Write comments in Turkish.
4. Output ONLY this JSON structure, nothing else:

{""recommendations"":[id1,id2,id3],""comments"":[{""EventId"":id1,""Comment"":""Turkish comment""},{""EventId"":id2,""Comment"":""Turkish comment""},{""EventId"":id3,""Comment"":""Turkish comment""}]}"
                    },
                    new { role = "user", content = prompt }
                },
                temperature = 0.2
            };

            var requestBody = JsonSerializer.Serialize(requestContent);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseContent);
            var rawContent = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            // ✅ DÜZELTME: GPT bazen markdown ```json ... ``` içinde döndürür, temizle
            return ExtractJsonFromResponse(rawContent);
        }

        /// <summary>
        /// GPT yanıtından saf JSON'u çıkarır.
        /// 1. Saf JSON: {"recommendations": [...]}  → direkt döndür
        /// 2. Markdown: ```json\n{...}\n```  → code block içini çıkar
        /// 3. Metin + JSON: "Açıklama...\n{...}"  → ilk { ile son } arasını al
        /// </summary>
        private static string ExtractJsonFromResponse(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
                return rawResponse;

            var trimmed = rawResponse.Trim();

            // Zaten saf JSON ise direkt döndür
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                return trimmed;

            // ```json ... ``` bloğunu bul ve çıkar
            var codeBlockMatch = Regex.Match(trimmed, @"```(?:json)?\s*\n?([\s\S]*?)\n?\s*```");
            if (codeBlockMatch.Success)
            {
                var extracted = codeBlockMatch.Groups[1].Value.Trim();
                if (extracted.StartsWith("{") && extracted.EndsWith("}"))
                    return extracted;
            }

            // Son çare: İlk { ile son } arasını al
            var firstBrace = trimmed.IndexOf('{');
            var lastBrace = trimmed.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            // Hiçbiri çalışmadıysa orijinali döndür
            return trimmed;
        }
    }
}