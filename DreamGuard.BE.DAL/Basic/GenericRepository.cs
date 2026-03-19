using DreamGuard.BE.DAL.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Basic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class 
    {
        protected readonly DreamGuardContext _context;
        public GenericRepository(DreamGuardContext context)
        {
            _context = context;
        }
        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _context.Set<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving data: {ex.Message}");
            }
        }
        public async Task<int> CreateAsync(T entity)
        {
            try
            {
                _context.Add(entity);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating entity: {ex.Message}");
            }
        }
        public async Task<int> UpdateAsync(T entity)
        {
            try
            {
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    _context.Set<T>().Attach(entity);
                    entry.State = EntityState.Modified;
                }
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating entity: {ex.Message}");
            }
        }
        public async Task<bool> RemoveAsync(T entity)
        {
            try
            {
                _context.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing entity: {ex.Message}");
            }
        }
        public async Task<T> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity by ID: {ex.Message}");
            }
        }

        public void AddEntity(T entity)
        {
            _context.Set<T>().Add(entity);
        }
        public void UpdateEntity(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public void RemoveEntity(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }
    }
}
