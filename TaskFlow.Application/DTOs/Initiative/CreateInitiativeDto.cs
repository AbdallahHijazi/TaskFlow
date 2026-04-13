using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Initiative
{
public class CreateInitiativeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Progress { get; set; } = 0;
        public bool IsAISuggested { get; set; } = false;
        public Guid? ImageId { get; set; }
        public Guid CreatedBy { get; set; }
        /// <summary>يُعبأ عند التحديث (PUT) لتحديد من عدّل السجل؛ يُتجاهل عند الإنشاء.</summary>
        public Guid? UpdatedBy { get; set; }
    }
}
