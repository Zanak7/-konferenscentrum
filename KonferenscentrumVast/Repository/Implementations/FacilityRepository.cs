using System;
using Microsoft.EntityFrameworkCore;
using KonferenscentrumVast.Data;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Repository.Implementations
{
    /// <summary>
    /// Entity Framework implementation of facility repository.
    /// Provides both active and all facility queries for different business scenarios.
    /// </summary>
    public class FacilityRepository : IFacilityRepository
    {
        private readonly ApplicationDbContext _context;

        public FacilityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Facility>> GetAllAsync()
        {
            return await _context.Facilities
                .Include(f => f.Bookings)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves only active facilities that can be booked.
        /// Used for customer-facing booking interfaces.
        /// </summary>
        public async Task<IEnumerable<Facility>> GetActiveAsync()
        {
            return await _context.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Facility?> GetByIdAsync(int id)
        {
            return await _context.Facilities
                .Include(f => f.Bookings)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Facility> CreateAsync(Facility facility)
        {
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();
            return facility;
        }

        public async Task<Facility?> UpdateAsync(int id, Facility facility)
        {
            var existingFacility = await _context.Facilities.FindAsync(id);
            if (existingFacility == null) return null;

            existingFacility.Name = facility.Name;
            existingFacility.Description = facility.Description;
            existingFacility.Address = facility.Address;
            existingFacility.PostalCode = facility.PostalCode;
            existingFacility.City = facility.City;
            existingFacility.MaxCapacity = facility.MaxCapacity;
            existingFacility.PricePerDay = facility.PricePerDay;
            existingFacility.IsActive = facility.IsActive;
            existingFacility.ImagePaths = facility.ImagePaths;

            await _context.SaveChangesAsync();
            return existingFacility;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null) return false;

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Facilities.AnyAsync(f => f.Id == id);
        }
    }
}

