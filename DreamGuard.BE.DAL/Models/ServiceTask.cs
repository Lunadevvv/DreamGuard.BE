using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServiceTask
    {
        public Guid ServiceTaskId { get; set; } = Guid.NewGuid();
        public Guid StaffId { get; set; }
        public Guid SoId { get; set; }
        public string Status { get; set; } = ServiceTaskStatus.Pending;
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public Staff Staff { get; set; } = null!;
        public ServiceOrder ServiceOrder { get; set; } = null!;
        public ICollection<ServiceEvidence> ServiceEvidences { get; set; } = new List<ServiceEvidence>();
    }
}
