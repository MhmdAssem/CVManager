using CV_Manager.Domain.Interfaces;
using CV_Manager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CV_Manager.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation that provides basic CRUD operations
    /// and consistent return types for querying entities
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CVManagerContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(CVManagerContext context, ILogger<Repository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Retrieves all entities of type T from the database
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all entities of type {EntityType}", typeof(T).Name);
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Retrieves entities by ID, returning an IEnumerable to maintain consistency
        /// even though it typically will contain only one item
        /// </summary>
        public virtual async Task<T> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving entity of type {EntityType} with ID: {Id}",
                    typeof(T).Name, id);

                // FindAsync is the most efficient way to get an entity by its primary key
                // It first checks the EF change tracker and only hits the database if needed
                var entity = await _dbSet.FindAsync(id);

                if (entity == null)
                {
                    _logger.LogInformation("No entity of type {EntityType} found with ID: {Id}",
                        typeof(T).Name, id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved entity of type {EntityType} with ID: {Id}",
                        typeof(T).Name, id);
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred while retrieving entity of type {EntityType} with ID: {Id}",
                    typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Adding new entity of type {EntityType}", typeof(T).Name);
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added new entity of type {EntityType}", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing entity in the database
        /// </summary>
        public virtual async Task UpdateAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Updating entity of type {EntityType}", typeof(T).Name);

                // Explicitly mark the entity as modified
                _context.Entry(entity).State = EntityState.Modified;

                // Save changes
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated entity of type {EntityType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Deletes an entity from the database by ID
        /// </summary>
        public virtual async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting entity of type {EntityType} with ID: {Id}", typeof(T).Name, id);
                var entity = await GetByIdAsync(id);

                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted entity of type {EntityType} with ID: {Id}",
                        typeof(T).Name, id);
                }
                else
                {
                    _logger.LogWarning("No entity of type {EntityType} found with ID: {Id} for deletion",
                        typeof(T).Name, id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting entity of type {EntityType} with ID: {Id}",
                    typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Checks if an entity with the specified ID exists in the database
        /// </summary>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                return entity != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of entity of type {EntityType} with ID: {Id}",
                    typeof(T).Name, id);
                throw;
            }
        }
    }
}