using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Infrastructure.AI
{
    public record OllamaMessage(string role, string content);
    public record OllamaChatRequest(
        string model,
        bool stream,
        List<OllamaMessage> messages
    );

    public class OllamaChatResponse
    {
        public OllamaMessage? message { get; set; }
    }
}
