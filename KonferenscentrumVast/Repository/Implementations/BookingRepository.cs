using System;
using Microsoft.EntityFrameworkCore;
using KonferenscentrumVast.Data;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Repository.Implementations
{
    /// <summary>
    /// Entity Framework implementation of booking repository.
    /// Handles complex date overlap queries for availability checking.
    /// </summary>
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Facility)
                .Include(b => b.Contract)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Facility)
                .Include(b => b.Contract)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .Include(b => b.Contract)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByFacilityIdAsync(int facilityId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Contract)
                .Where(b => b.FacilityId == facilityId)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// Finds all bookings that overlap with the specified date range.
        /// Uses date overlap logic: (Start <= end) AND (End >= start)
        /// </summary>
        public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // overlaps if (Start <= end) AND (End >= start)
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Facility)
                .Where(b => b.StartDate <= endDate && b.EndDate >= startDate)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> UpdateAsync(int id, Booking booking)
        {
            var existingBooking = await _context.Bookings.FindAsync(id);
            if (existingBooking == null) return null;

            existingBooking.CustomerId = booking.CustomerId;
            existingBooking.FacilityId = booking.FacilityId;
            existingBooking.StartDate = booking.StartDate;
            existingBooking.EndDate = booking.EndDate;
            existingBooking.NumberOfParticipants = booking.NumberOfParticipants;
            existingBooking.Notes = booking.Notes;
            existingBooking.Status = booking.Status;
            existingBooking.TotalPrice = booking.TotalPrice;
            existingBooking.ConfirmedDate = booking.ConfirmedDate;
            existingBooking.CancelledDate = booking.CancelledDate;

            await _context.SaveChangesAsync();
            return existingBooking;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Bookings.AnyAsync(b => b.Id == id);
        }

        /// <summary>
        /// Checks if any existing bookings overlap with the specified time period for a facility.
        /// Uses optimized query with AsNoTracking for performance.
        /// Overlap logic: (existing.Start < newEnd) AND (existing.End > newStart)
        /// </summary>
        public async Task<bool> HasOverlapAsync(int facilityId, DateTime startDate, DateTime endDate, BookingStatus[] statuses)
        {
            return await _context.Bookings
                .AsNoTracking()
                .Where(b => b.FacilityId == facilityId && statuses.Contains(b.Status))
                .AnyAsync(b => b.StartDate < endDate && b.EndDate > startDate);
        }

        /// <summary>
        /// Checks for booking overlaps while excluding a specific booking (used for rescheduling).
        /// Prevents a booking from conflicting with itself during updates.
        /// </summary>
        public async Task<bool> HasOverlapExcludingAsync(int bookingId, int facilityId, DateTime startDate, DateTime endDate, BookingStatus[] statuses)
        {
            return await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Id != bookingId && b.FacilityId == facilityId && statuses.Contains(b.Status))
                .AnyAsync(b => b.StartDate < endDate && b.EndDate > startDate);
        }
    }
}