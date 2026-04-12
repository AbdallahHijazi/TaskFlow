using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key)
                    : base($"لم يتم العثور على {entityName} بالمعرف: {key}")
        {
        }

    }
}
