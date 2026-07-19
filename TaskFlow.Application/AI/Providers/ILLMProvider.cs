using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.AI.Models;

namespace TaskFlow.Application.AI.Providers
{
    public interface ILLMProvider
    {
        Task<LLMResponse> ExecuteAsync(
            LLMRequest request,
            CancellationToken cancellationToken = default
            );
    }
}
