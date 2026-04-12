using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Status
{
    public class CreateStatusDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
    }
}
