using System;
using System.Collections.Generic;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.BLL.Responses
{
    public class ShippingTaskResponse
    {
        public Guid ShippingTaskId { get; set; }
        public Guid StaffId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TradeInOrderId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ShippingDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string StaffNote { get; set; } = string.Empty;
        public List<DamagedItemResponse> DamagedItems { get; set; } = new List<DamagedItemResponse>();
        public List<ShippingEvidenceResponse> Evidences { get; set; } = new List<ShippingEvidenceResponse>();

    }
    public class DamagedItemResponse
    {
        public Guid OrderItemId { get; set; }
        public int DamagedQuantity { get; set; }
    }

    public class ShippingEvidenceResponse
    {
        public Guid EvidenceId { get; set; }
        public string EvidenceUrl { get; set; } = string.Empty;
        public string EvidenceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
