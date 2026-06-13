using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Infrastructure.AI
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = "";
        public string Model { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 300;
    }
}
