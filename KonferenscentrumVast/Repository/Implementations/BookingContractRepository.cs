using System;
using Microsoft.EntityFrameworkCore;
using KonferenscentrumVast.Data;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Repository.Implementations
{
    /// <summary>
    /// Entity Framework implementation of booking contract repository.
    /// Handles contract data persistence with booking relationship management.
    /// </summary>
    public class BookingContractRepository : IBookingContractRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingContract>> GetAllAsync()
        {
            return await _context.BookingContracts
                .Include(c => c.Booking)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<BookingContract?> GetByIdAsync(int id)
        {
            return await _context.BookingContracts
                .Include(c => c.Booking)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<BookingContract?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.BookingContracts
                .Include(c => c.Booking)
                .FirstOrDefaultAsync(c => c.BookingId == bookingId);
        }

        public async Task<BookingContract> CreateAsync(BookingContract contract)
        {
            _context.BookingContracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<BookingContract?> UpdateAsync(int id, BookingContract contract)
        {
            var existing = await _context.BookingContracts.FindAsync(id);
            if (existing == null) return null;

            existing.Version = contract.Version;
            existing.Status = contract.Status;
            existing.Terms = contract.Terms;
            existing.TotalAmount = contract.TotalAmount;
            existing.Currency = contract.Currency;
            existing.PaymentDueDate = contract.PaymentDueDate;
            existing.LastUpdated = contract.LastUpdated;
            existing.SignedAt = contract.SignedAt;
            existing.CancelledAt = contract.CancelledAt;
            existing.CancelReason = contract.CancelReason;
            existing.CustomerName = contract.CustomerName;
            existing.CustomerEmail = contract.CustomerEmail;
            existing.FacilityName = contract.FacilityName;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BookingContracts.FindAsync(id);
            if (entity == null) return false;

            _context.BookingContracts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByBookingIdAsync(int bookingId)
        {
            var entity = await _context.BookingContracts
                .FirstOrDefaultAsync(c => c.BookingId == bookingId);
            if (entity == null) return false;

            _context.BookingContracts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.BookingContracts.AnyAsync(c => c.Id == id);
        }
    }
}