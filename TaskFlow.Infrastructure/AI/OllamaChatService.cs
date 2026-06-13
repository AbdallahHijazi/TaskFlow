using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;

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
            var request = new OllamaChatRequest(
                model: _options.Model,
                stream: false,
                messages:
                [
                    new OllamaMessage(
                    "system",
                    "You are an AI assistant for TaskFlow, a smart task and initiative management system."
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

            return result?.message?.content ?? "";
        }
    }
}
