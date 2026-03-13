using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace DreamGuard.BE.DAL.Basic
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync(); 
        Task<int> SaveChangeAsync();
    }
}
