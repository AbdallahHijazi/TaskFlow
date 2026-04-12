using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TaskFlow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? ImageId { get; set; }

        public Role? Role { get; set; }
        public Image? Image { get; set; }

        public ICollection<Image> UploadedImages { get; set; } = new List<Image>();
        public ICollection<Initiative> AssignedInitiatives { get; set; } = new List<Initiative>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
