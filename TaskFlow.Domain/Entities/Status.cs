using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Entities
{
    public class Status
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }

        public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
