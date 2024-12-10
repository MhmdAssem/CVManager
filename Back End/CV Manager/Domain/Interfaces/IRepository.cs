﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace CV_Manager.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}