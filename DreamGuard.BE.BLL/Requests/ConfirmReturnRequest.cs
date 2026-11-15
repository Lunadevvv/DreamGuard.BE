using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Requests
{
    public class ConfirmReturnRequest
    {
        public bool IsRefund { get; set; } = true;

        public string? ManagerNote { get; set; }

        public List<DamagedItemRequest> DamagedItems { get; set; } = new List<DamagedItemRequest>();
    }
}
