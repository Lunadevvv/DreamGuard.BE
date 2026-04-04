using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Constants
{
    public static class Role
    {
        public const string Admin = nameof(Admin);
        public const string Manager = nameof(Manager);
        public const string Seller = nameof(Seller);
        public const string CleaningStaff = nameof(CleaningStaff);
        public const string User = nameof(User);
        public const string DeliveryStaff = nameof(DeliveryStaff);
    }
}
