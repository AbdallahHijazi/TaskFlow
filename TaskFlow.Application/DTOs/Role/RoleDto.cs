using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Role
{
    public class RoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
