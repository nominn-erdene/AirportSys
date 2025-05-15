using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Airport.Data.Interfaces;

namespace Airport.Server.Services
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> _items = new();

        public Task<T> GetByIdAsync(int id)
        {
            return Task.FromResult(_items.FirstOrDefault());
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_items.AsEnumerable());
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(_items.AsQueryable().FirstOrDefault(predicate));
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(_items.AsQueryable().Where(predicate).AsEnumerable());
        }

        public Task AddAsync(T entity)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> SaveChangesAsync()
        {
            return Task.FromResult(true);
        }
    }
} 