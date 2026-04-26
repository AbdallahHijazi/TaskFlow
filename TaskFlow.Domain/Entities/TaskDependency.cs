using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class TaskDependency : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? DependencyTypeId { get; set; }
        public Guid? PredecessorId { get; set; }
        public Guid? SuccessorId { get; set; }
        public DependencyType? DependencyType { get; set; }
        public TaskItem? Predecessor { get; set; }
        public TaskItem? Successor { get; set; }
    }
}
