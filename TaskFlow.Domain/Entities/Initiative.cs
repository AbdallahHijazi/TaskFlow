using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class Initiative : BaseEntity, ITenantEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Progress { get; set; }
        public bool? IsAISuggested { get; set; }
        public bool? IsActive { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public Guid? StatusId { get; set; }
        public Guid? AssignedToId { get; set; }
        public Guid? ImageId { get; set; }
        public Status? Status { get; set; }
        public User? AssignedTo { get; set; }
        public Image? Image { get; set; }
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
