using System;
using KonferenscentrumVast.Models;

namespace KonferenscentrumVast.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for managing customer data operations.
    /// Includes email-based lookup for customer identification.
    /// </summary>
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer?> UpdateAsync(int id, Customer customer);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

