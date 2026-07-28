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

            var messages = new List<OllamaMessage>();

            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(
                    new OllamaMessage(
                        "system",
                        request.SystemPrompt));
            }

            messages.Add(
                new OllamaMessage(
                    "user",
                    request.Prompt));

            var ollamaRequest = new OllamaChatRequest(
                model: _options.Model,
                stream: false,
                messages: messages);

            using var response = await _httpClient.PostAsJsonAsync(
                "api/chat",
                ollamaRequest,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                    cancellationToken: cancellationToken);

            if (result?.message?.content is null)
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty response.");
            }

            return new LLMResponse
            {
                Content = result.message.content
            };
        }
    }
}
