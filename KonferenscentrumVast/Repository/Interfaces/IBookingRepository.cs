using System;
using KonferenscentrumVast.Models;

namespace KonferenscentrumVast.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for managing booking data operations.
    /// Includes specialized methods for availability checking and date range queries.
    /// </summary>
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Booking>> GetByFacilityIdAsync(int facilityId);
        Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking?> UpdateAsync(int id, Booking booking);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasOverlapAsync(int facilityId, DateTime startDate, DateTime endDate, BookingStatus[] statuses);
        Task<bool> HasOverlapExcludingAsync(int bookingId, int facilityId, DateTime startDate, DateTime endDate, BookingStatus[] statuses);
    }
}