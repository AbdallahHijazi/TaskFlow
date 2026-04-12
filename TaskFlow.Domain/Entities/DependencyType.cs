using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Entities
{
    public class DependencyType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<TaskDependency> TaskDependencies { get; set; } = new List<TaskDependency>();
    }
}
