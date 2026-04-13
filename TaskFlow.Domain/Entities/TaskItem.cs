using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TaskFlow.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Progress { get; set; }
        public bool? IsAISuggested { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public Guid? InitiativeId { get; set; }
        public Guid? ImageId { get; set; }
        public Guid? StatusId { get; set; }
        public Guid? AssignedToId { get; set; }
        public Guid? ParentId { get; set; }

        public Initiative? Initiative { get; set; }
        public Image? Image { get; set; }
        public Status? Status { get; set; }
        public User? AssignedTo { get; set; }

        public TaskItem? Parent { get; set; }
        public ICollection<TaskItem> Children { get; set; } = new List<TaskItem>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TaskDependency> PredecessorDependencies { get; set; } = new List<TaskDependency>();
        public ICollection<TaskDependency> SuccessorDependencies { get; set; } = new List<TaskDependency>();
    }
}
