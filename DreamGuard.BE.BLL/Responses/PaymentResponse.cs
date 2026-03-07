using System;
using System.Collections.Generic;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.BLL.Responses
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid? POrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }

    public class PaymentSummaryResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentResponse
    {
        public Guid PaymentId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
