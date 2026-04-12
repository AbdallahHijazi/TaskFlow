using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Exceptions
{
    public class StatusAlreadyExistsException : Exception
    {
        public StatusAlreadyExistsException(string name)
            : base($"الحالة باسم '{name}' موجودة مسبقاً")
        {
        }
    }
}
