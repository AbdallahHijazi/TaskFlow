using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.AI.Models;

namespace TaskFlow.Infrastructure.AI
{

    public class OllamaChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;

        public OllamaChatService(
            HttpClient httpClient,
            IOptions<OllamaOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> SendMessageAsync(
            string message,
            CancellationToken cancellationToken = default)
        {
            var language = GenerationLanguageDetector.Detect(message);
            var languageName = GenerationLanguageDetector.Name(language);
            var request = new OllamaChatRequest(
                model: _options.Model,
                stream: false,
                messages:
                [
                    new OllamaMessage(
                    "system",
                    $"You are an AI assistant for TaskFlow, a smart task and initiative management system. Respond in {languageName} only. Never answer in Chinese or any third language. Technical identifiers and code may remain unchanged."
                ),
                new OllamaMessage("user", message)
                ]
            );

            var response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: cancellationToken);

            var content = result?.message?.content ?? "";
            if (GenerationLanguageDetector.Matches(content, language)) return content;

            var correctionRequest = new OllamaChatRequest(
                _options.Model,
                false,
                [
                    new OllamaMessage("system", $"Rewrite answers in {languageName} only. Never use Chinese or any third language."),
                    new OllamaMessage("user", message),
                    new OllamaMessage("assistant", content),
                    new OllamaMessage("user", $"Rewrite your answer in {languageName} only without changing its meaning.")
                ]);
            var correctionResponse = await _httpClient.PostAsJsonAsync("/api/chat", correctionRequest, cancellationToken);
            correctionResponse.EnsureSuccessStatusCode();
            var corrected = await correctionResponse.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
            var correctedContent = corrected?.message?.content ?? "";
            if (!GenerationLanguageDetector.Matches(correctedContent, language))
                throw new InvalidOperationException($"The model did not return a valid {languageName} response.");
            return correctedContent;
        }
    }
}
