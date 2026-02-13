using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Basic
{
    public interface IGenericRepository<T> where T : class 
    {
        Task<List<T>> GetAllAsync();

        Task<int> CreateAsync(T entity);

        Task<int> UpdateAsync(T entity);

        Task<bool> RemoveAsync(T entity);

        Task<T> GetByIdAsync(Guid id);

    }
}
