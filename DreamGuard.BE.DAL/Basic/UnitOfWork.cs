using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.DbContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace DreamGuard.BE.DAL.Basic
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DreamGuardContext _context;

        public UnitOfWork(DreamGuardContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
