using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Constants
{
    public static class OrderServiceStatus
    {
        public const string Pending = nameof(Pending);
        public const string Rejected = nameof(Rejected);
        public const string Cancelled = nameof(Cancelled);
        public const string Confirmed = nameof(Confirmed);
        public const string Processing = nameof(Processing);
        public const string Completed = nameof(Completed);
        public const string Refund = nameof(Refund);
        public const string ForcedCancelled = nameof(ForcedCancelled);
    }
}
