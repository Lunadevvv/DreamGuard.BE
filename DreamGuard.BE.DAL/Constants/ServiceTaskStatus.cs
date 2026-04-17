using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Constants
{
    public static class ServiceTaskStatus
    {
        public const string Pending = nameof(Pending);
        public const string Cancelled = nameof(Cancelled);
        public const string CheckedIn = nameof(CheckedIn);
        public const string CheckedOut = nameof(CheckedOut);
        public const string Processing = nameof(Processing);
        public const string Completed = nameof(Completed);
        public const string ForcedCancelled = nameof(ForcedCancelled);
        public const string Rescheduled = nameof(Rescheduled);
    }
}
