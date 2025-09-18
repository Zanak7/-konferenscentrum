using System;
using Microsoft.EntityFrameworkCore;
using KonferenscentrumVast.Data;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Repository.Implementations
{
    /// <summary>
    /// Entity Framework implementation of customer repository.
    /// Includes normalized email lookup for case-insensitive searches.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.Bookings)
                .OrderBy(c => c.LastName)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Finds customer by email address using case-insensitive comparison.
        /// Normalizes input email to lowercase for consistent matching.
        /// </summary>
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.ToLower() == normalized);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> UpdateAsync(int id, Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null) return null;

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.CompanyName = customer.CompanyName;
            existingCustomer.Address = customer.Address;
            existingCustomer.PostalCode = customer.PostalCode;
            existingCustomer.City = customer.City;

            await _context.SaveChangesAsync();
            return existingCustomer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }
    }
}