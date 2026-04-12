using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Entities
{
    public class Role
    {
        public Guid RoleId { get; set; } = Guid.NewGuid();
        public string? RoleName { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
