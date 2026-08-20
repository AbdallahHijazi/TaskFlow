using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.AI.Models;
using TaskFlow.Application.AI.Providers;

namespace TaskFlow.Infrastructure.AI
{
    public sealed class OllamaProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;

        public OllamaProvider(
            HttpClient httpClient,
            IOptions<OllamaOptions> options)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(options);

            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<LLMResponse> ExecuteAsync(
            LLMRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new ArgumentException(
                    "Prompt cannot be empty.",
                    nameof(request));
            }

            var language = request.OutputLanguage ?? GenerationLanguageDetector.Detect(request.Prompt);
            var languageName = GenerationLanguageDetector.Name(language);
            var messages = new List<OllamaMessage>();

            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(
                    new OllamaMessage(
                        "system",
                        $"{request.SystemPrompt}\n\nLANGUAGE POLICY: All human-readable generated content must be in {languageName} only. Do not use Chinese or any third language. JSON property names, identifiers, enum values, code, and proper names may remain unchanged."));
            }

            messages.Add(
                new OllamaMessage(
                    "user",
                    request.Prompt));

            if (string.IsNullOrWhiteSpace(request.SystemPrompt))
                messages.Insert(0, new OllamaMessage("system", $"Respond in {languageName} only. Never use Chinese or any third language."));

            var content = await SendAsync(messages, cancellationToken);
            if (!GenerationLanguageDetector.Matches(content, language))
            {
                messages.Add(new OllamaMessage("assistant", content));
                messages.Add(new OllamaMessage("user", $"Rewrite the response in {languageName} only. Preserve the exact requested structure and data. Return no commentary."));
                content = await SendAsync(messages, cancellationToken);
            }

            if (!GenerationLanguageDetector.Matches(content, language))
                throw new InvalidOperationException($"The model did not return a valid {languageName} response.");

            return new LLMResponse { Content = content };
        }

        private async Task<string> SendAsync(List<OllamaMessage> messages, CancellationToken cancellationToken)
        {
            var ollamaRequest = new OllamaChatRequest(_options.Model, false, messages);

            using var response = await _httpClient.PostAsJsonAsync(
                "api/chat",
                ollamaRequest,
                cancellationToken);

            //response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                throw new Exception(
                    $"Ollama Error: {error}");
            }

            var result =
                await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                    cancellationToken: cancellationToken);

            if (result?.message?.content is null)
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty response.");
            }

            return result.message.content;
        }
    }
}
