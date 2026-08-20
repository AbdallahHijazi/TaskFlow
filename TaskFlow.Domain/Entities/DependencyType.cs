using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class DependencyType : BaseEntity, ITenantEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<TaskDependency> TaskDependencies { get; set; } = new List<TaskDependency>();
    }
}
