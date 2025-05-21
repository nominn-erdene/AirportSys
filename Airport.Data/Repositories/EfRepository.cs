using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Airport.Data.Interfaces;
using Airport.Core.Models;
using Microsoft.Extensions.Logging;

namespace Airport.Data.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly AirportDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ILogger<EfRepository<T>> _logger;

        public EfRepository(AirportDbContext context, ILogger<EfRepository<T>> logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var query = _dbSet.AsQueryable();

            // Include related entities based on the type
            if (typeof(T) == typeof(Flight))
            {
                query = query.Include("Seats");
            }
            else if (typeof(T) == typeof(Passenger))
            {
                query = query.Include("Flight")
                            .Include("AssignedSeat");
            }
            else if (typeof(T) == typeof(Seat))
            {
                query = query.Include("Flight")
                            .Include("Passenger");
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogInformation($"Executing query for type {typeof(T).Name} with predicate: {predicate}");
                var query = _dbSet.AsQueryable();

                // Include related entities for Passenger queries
                if (typeof(T) == typeof(Passenger))
                {
                    _logger.LogInformation("Including Flight and AssignedSeat for Passenger query");
                    query = query.Include("Flight")
                                .Include("AssignedSeat");
                }
                // Include related entities for Flight queries
                else if (typeof(T) == typeof(Flight))
                {
                    _logger.LogInformation("Including Seats for Flight query");
                    query = query.Include("Seats");
                }
                // Include related entities for Seat queries
                else if (typeof(T) == typeof(Seat))
                {
                    _logger.LogInformation("Including Flight and Passenger for Seat query");
                    query = query.Include("Flight")
                                .Include("Passenger");
                }

                var result = await query.FirstOrDefaultAsync(predicate);
                _logger.LogInformation($"Query result: {(result == null ? "null" : "found")}");
                
                if (result != null && typeof(T) == typeof(Passenger))
                {
                    var passenger = result as Passenger;
                    _logger.LogInformation($"Passenger details - Name: {passenger.Name}, " +
                        $"Flight: {passenger.Flight?.FlightNumber ?? "null"}, " +
                        $"Seat: {passenger.AssignedSeat?.SeatNumber ?? "null"}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing query for type {typeof(T).Name}");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var query = _dbSet.AsQueryable();

            // Include related entities based on the type
            if (typeof(T) == typeof(Flight))
            {
                query = query.Include("Seats");
            }
            else if (typeof(T) == typeof(Passenger))
            {
                query = query.Include("Flight")
                            .Include("AssignedSeat");
            }
            else if (typeof(T) == typeof(Seat))
            {
                query = query.Include("Flight")
                            .Include("Passenger");
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.AsQueryable();

            // Include related entities based on the type
            if (typeof(T) == typeof(Flight))
            {
                query = query.Include("Seats");
            }
            else if (typeof(T) == typeof(Passenger))
            {
                query = query.Include("Flight")
                            .Include("AssignedSeat");
            }
            else if (typeof(T) == typeof(Seat))
            {
                query = query.Include("Flight")
                            .Include("Passenger");
            }

            return await query.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 