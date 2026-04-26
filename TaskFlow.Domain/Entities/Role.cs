using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class Role : BaseEntity
    {
        public Guid RoleId { get; set; } = Guid.NewGuid();
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
