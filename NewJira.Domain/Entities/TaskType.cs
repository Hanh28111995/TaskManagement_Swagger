using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJira.Domain.Entities
{
    public class TaskType
    {
        public int Id { get; set; }

        public string TaskTypeName { get; set; } = string.Empty;
    }
}
