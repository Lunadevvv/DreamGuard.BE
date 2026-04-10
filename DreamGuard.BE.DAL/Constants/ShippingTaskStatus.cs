using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Constants
{
    public static class ShippingTaskStatus
    {
        public const string Pending = nameof(Pending);
        public const string Delivering = nameof(Delivering);
        public const string Arrived = nameof(Arrived);
        public const string Delivered = nameof(Delivered);
        public const string Returned = nameof(Returned);
        public const string Cancelled = nameof(Cancelled);
        public const string Returning = nameof(Returning);
        public const string RefundedAndRestocked = nameof(RefundedAndRestocked);
        public const string RefundedAndDamaged = nameof(RefundedAndDamaged);
        public const string ExchangeRequested = nameof(ExchangeRequested);
    }
}
