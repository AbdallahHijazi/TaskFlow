using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface IAiChatService
    {
        Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    }
}
