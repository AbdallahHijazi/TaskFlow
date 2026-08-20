using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.AI.Models
{
    public sealed class LLMRequest
    {
        public string Prompt { get; init; } = string.Empty;
        public string? SystemPrompt { get; init; }
        public GenerationLanguage? OutputLanguage { get; init; }
    }
}
