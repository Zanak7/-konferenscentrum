using System;
using KonferenscentrumVast.Exceptions;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Validation;

namespace KonferenscentrumVast.Services
{
    /// <summary>
    /// Manages customer data with email uniqueness enforcement and booking relationship validation.
    /// Handles customer lifecycle operations with automatic email normalization.
    /// </summary>
    public class CustomerService
    {
        private readonly ICustomerRepository _customers;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customers, ILogger<CustomerService> logger)
        {
            _customers = customers;
            _logger = logger;
        }

        public Task<IEnumerable<Customer>> GetAllAsync() => _customers.GetAllAsync();

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _customers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Customer with id={id} was not found.");
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var existing = await _customers.GetByEmailAsync(NormalizeEmail(email));
            return existing;
        }

        public async Task<Customer> CreateAsync(
            string firstName,
            string lastName,
            string email,
            string? phone,
            string? companyName,
            string? address,
            string? postalCode,
            string? city)
        {
            var displayName = $"{firstName} {lastName}".Trim();
            CustomerValidator.ValidateCustomer(displayName, email);

            var normalizedEmail = NormalizeEmail(email);
            var duplicate = await _customers.GetByEmailAsync(normalizedEmail);
            if (duplicate != null)
                throw new ConflictException($"A customer with email '{normalizedEmail}' already exists (id={duplicate.Id}).");

            var customer = new Customer
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = normalizedEmail,
                Phone = phone?.Trim() ?? string.Empty,
                CompanyName = companyName?.Trim() ?? string.Empty,
                Address = address?.Trim() ?? string.Empty,
                PostalCode = postalCode?.Trim() ?? string.Empty,
                City = city?.Trim() ?? string.Empty,
                CreatedDate = DateTime.UtcNow
            };

            customer = await _customers.CreateAsync(customer);
            _logger.LogInformation("Created customer {CustomerId} ({Email}).", customer.Id, customer.Email);
            return customer;
        }

        public async Task<Customer> UpdateAsync(
            int id,
            string firstName,
            string lastName,
            string email,
            string? phone,
            string? companyName,
            string? address,
            string? postalCode,
            string? city)
        {
            var existing = await _customers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Customer with id={id} was not found.");

            var displayName = $"{firstName} {lastName}".Trim();
            CustomerValidator.ValidateCustomer(displayName, email);

            var normalizedEmail = NormalizeEmail(email);
            if (!string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await _customers.GetByEmailAsync(normalizedEmail);
                if (duplicate != null && duplicate.Id != id)
                    throw new ConflictException($"A customer with email '{normalizedEmail}' already exists (id={duplicate.Id}).");
            }

            existing.FirstName = firstName.Trim();
            existing.LastName = lastName.Trim();
            existing.Email = normalizedEmail;
            existing.Phone = phone?.Trim() ?? string.Empty;
            existing.CompanyName = companyName?.Trim() ?? string.Empty;
            existing.Address = address?.Trim() ?? string.Empty;
            existing.PostalCode = postalCode?.Trim() ?? string.Empty;
            existing.City = city?.Trim() ?? string.Empty;

            var updated = await _customers.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Customer with id={id} was not found during update.");

            _logger.LogInformation("Updated customer {CustomerId} ({Email}).", updated.Id, updated.Email);
            return updated;
        }

        /// <summary>
        /// Deletes customer with safety check for active bookings.
        /// Prevents deletion if customer has pending or confirmed bookings.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var existing = await _customers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Customer with id={id} was not found.");

            if (existing.Bookings?.Any(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) == true)
                throw new ConflictException("Customer has active bookings and cannot be deleted.");

            var removed = await _customers.DeleteAsync(id);
            if (!removed)
                throw new NotFoundException($"Customer with id={id} was not found during delete.");

            _logger.LogInformation("Deleted customer {CustomerId}.", id);
        }

        /// <summary>
        /// Normalizes email addresses to lowercase for consistent storage and comparison.
        /// Ensures case-insensitive email uniqueness enforcement.
        /// </summary>
        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}