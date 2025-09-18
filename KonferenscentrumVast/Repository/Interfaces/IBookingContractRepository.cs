using System;
using KonferenscentrumVast.Models;

namespace KonferenscentrumVast.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for managing booking data operations.
    /// </summary>
    /// <remarks>
    /// This interface follows the Repository Pattern, which separates data access logic from business logic.
    /// we use interfaces with implementations because:
    /// 
    /// 1. TESTABILITY: Allows mocking the repository in unit tests without requiring a real database.
    /// 
    /// 2. DEPENDENCY INVERSION: Services depend on abstractions (interfaces) rather than concrete classes.
    ///    This makes the code more flexible and follows SOLID principles.
    /// 
    /// 3. IMPLEMENTATION FLEXIBILITY: Could swap Entity Framework for Dapper, MongoDB, or web APIs
    ///    without changing service layer code, as long as the interface contract is maintained.
    /// 
    /// 4. DEPENDENCY INJECTION: The DI container can inject the appropriate implementation
    ///    (configured in Program.cs).
    /// 
    /// While this pattern might add complexity for simple applications, it's industry standard and essential
    /// for maintainable, testable enterprise applications.
    /// </remarks>
   
    /// <summary>
    /// Repository interface for managing booking contract data operations.
    /// Provides methods for CRUD operations and booking-specific queries.
    /// </summary>
    public interface IBookingContractRepository
    {
        Task<IEnumerable<BookingContract>> GetAllAsync();
        Task<BookingContract?> GetByIdAsync(int id);
        Task<BookingContract?> GetByBookingIdAsync(int bookingId);
        Task<BookingContract> CreateAsync(BookingContract contract);
        Task<BookingContract?> UpdateAsync(int id, BookingContract contract);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByBookingIdAsync(int bookingId);
        Task<bool> ExistsAsync(int id);
    }
}