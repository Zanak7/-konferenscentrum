using System;
using KonferenscentrumVast.Exceptions;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Services
{
    /// <summary>
    /// Manages conference facility operations including creation, updates, and activation status.
    /// Handles facility data validation and soft-delete through activation toggles.
    /// </summary>
    public sealed class FacilityService
    {
        private readonly IFacilityRepository _facilities;
        private readonly ILogger<FacilityService> _logger;

        public FacilityService(
            IFacilityRepository facilities,
            ILogger<FacilityService> logger)
        {
            _facilities = facilities;
            _logger = logger;
        }

        public Task<IEnumerable<Facility>> GetAllAsync() => _facilities.GetAllAsync();

        public Task<IEnumerable<Facility>> GetActiveAsync() => _facilities.GetActiveAsync();

        public async Task<Facility> GetByIdAsync(int id)
        {
            return await _facilities.GetByIdAsync(id)
                ?? throw new NotFoundException($"Facility with id={id} was not found.");
        }

        public async Task<Facility> CreateAsync(
            string name,
            string description,
            string address,
            string postalCode,
            string city,
            int maxCapacity,
            decimal pricePerDay,
            bool isActive)
        {
            EnsureFacilityFields(name, address, postalCode, city, maxCapacity, pricePerDay);

            var facility = new Facility
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Address = address.Trim(),
                PostalCode = postalCode.Trim(),
                City = city.Trim(),
                MaxCapacity = maxCapacity,
                PricePerDay = pricePerDay,
                IsActive = isActive,
                ImagePaths = string.Empty,
                CreatedDate = DateTime.UtcNow
            };

            facility = await _facilities.CreateAsync(facility);
            _logger.LogInformation("Created facility {FacilityId} ({Name}).", facility.Id, facility.Name);

            return facility;
        }

        public async Task<Facility> UpdateAsync(
            int id,
            string name,
            string description,
            string address,
            string postalCode,
            string city,
            int maxCapacity,
            decimal pricePerDay,
            bool isActive)
        {
            EnsureFacilityFields(name, address, postalCode, city, maxCapacity, pricePerDay);

            var existing = await _facilities.GetByIdAsync(id)
                ?? throw new NotFoundException($"Facility with id={id} was not found.");

            existing.Name = name.Trim();
            existing.Description = description?.Trim() ?? string.Empty;
            existing.Address = address.Trim();
            existing.PostalCode = postalCode.Trim();
            existing.City = city.Trim();
            existing.MaxCapacity = maxCapacity;
            existing.PricePerDay = pricePerDay;
            existing.IsActive = isActive;

            var updated = await _facilities.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Facility with id={id} was not found during update.");

            _logger.LogInformation("Updated facility {FacilityId}.", updated.Id);
            return updated;
        }

        public async Task DeleteAsync(int id)
        {
            var facility = await _facilities.GetByIdAsync(id)
                ?? throw new NotFoundException($"Facility with id={id} was not found.");


            var removed = await _facilities.DeleteAsync(id);
            if (!removed)
                throw new NotFoundException($"Facility with id={id} was not found during delete.");

            _logger.LogInformation("Deleted facility {FacilityId}.", id);
        }

        /// <summary>
        /// Toggles facility active status for soft delete functionality.
        /// Inactive facilities cannot be booked but preserve historical booking data.
        /// </summary>
        public async Task<Facility> SetActiveAsync(int facilityId, bool isActive)
        {
            var facility = await _facilities.GetByIdAsync(facilityId)
                ?? throw new NotFoundException($"Facility with id={facilityId} was not found.");

            facility.IsActive = isActive;

            var updated = await _facilities.UpdateAsync(facility.Id, facility)
                ?? throw new NotFoundException($"Facility with id={facility.Id} was not found during activation update.");

            _logger.LogInformation("Set facility {FacilityId} active={Active}.", facility.Id, isActive);
            return updated;
        }

        /// <summary>
        /// Validates required facility fields and business constraints.
        /// Ensures all mandatory data is present and values are within acceptable ranges.
        /// </summary>
        private static void EnsureFacilityFields(
            string name,
            string address,
            string postalCode,
            string city,
            int maxCapacity,
            decimal pricePerDay)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Facility name is required.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ValidationException("Address is required.");
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ValidationException("Postal code is required.");
            if (string.IsNullOrWhiteSpace(city))
                throw new ValidationException("City is required.");
            if (maxCapacity <= 0)
                throw new ValidationException("Max capacity must be greater than zero.");
            if (pricePerDay < 0)
                throw new ValidationException("Price per day cannot be negative.");
        }
    }
}