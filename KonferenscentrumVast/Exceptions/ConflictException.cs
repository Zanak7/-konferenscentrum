using System;
namespace KonferenscentrumVast.Exceptions
{
    /// <summary>
    /// Exception thrown when a business rule conflict occurs, such as double-booking a facility.
    /// Maps to HTTP 409 Conflict status code in the API response.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
        /// <summary>
        /// Creates a standardized booking conflict exception with formatted date range.
        /// </summary>
        public static ConflictException BookingConflict(DateTime start, DateTime end) =>
            new ConflictException($"Booking conflict between {start:u} and {end:u}.");
    }
}