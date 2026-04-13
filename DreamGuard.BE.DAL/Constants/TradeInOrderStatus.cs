using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Constants
{
    public enum TradeInOrderStatus
    {
        Pending = 0,
        WAITING_FOR_STAFF = 1,
        NEGOTIATING = 2,
        CONFIRMED = 3,
        PROCESSING = 4,
        DELIVERED = 5,
        COMPLETED = 6,
        CANCELLED = 7,
        REFUNDING = 8,
        REFUNDED = 9,
        ADMINCANCELLED = 10,
        RETURNING = 11,
        RefundedAndRestocked = 12,
        RefundedAndDamaged = 13,
        Shipping_Replacement = 14,
        FORCED_CANCELLED = 15
    }
}
