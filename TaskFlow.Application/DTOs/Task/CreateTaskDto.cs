using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Task
{
    public class CreateTaskDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Progress { get; set; } = 0;
        public Guid StatusId { get; set; }
        public Guid InitiativeId { get; set; }
        public Guid AssignedToId { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ImageId { get; set; }
        /// <summary>يُعبأ عند التحديث (PUT) لتحديد من عدّل السجل؛ يُتجاهل عند الإنشاء.</summary>
        public Guid? UpdatedById { get; set; }
    }
}
