using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.User
{
    public class UpdateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
