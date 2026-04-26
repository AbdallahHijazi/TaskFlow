using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Content { get; set; }
        public Guid? UserId { get; set; }
        public Guid? TaskId { get; set; }
        public User? User { get; set; }
        public TaskItem? Task { get; set; }
    }
}
