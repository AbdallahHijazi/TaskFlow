using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Common
{
    public class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
