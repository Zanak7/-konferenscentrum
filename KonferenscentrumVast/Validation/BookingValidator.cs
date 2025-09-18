using System;
using KonferenscentrumVast.Exceptions;

namespace KonferenscentrumVast.Validation
{
    // <summary>
    /// Validates booking creation and modification data to ensure business rules are met.
    /// Throws ValidationException when validation fails.
    /// </summary>
    public static class BookingValidator
    {
        /// <summary>
        /// Validates all data required to create a new booking.
        /// Ensures facility, room, attendees, dates, and customer information are valid.
        /// </summary>
        public static void EnsureCreateIsValid(
            int facilityId,
            int roomId,
            DateTime start,
            DateTime end,
            string customerName,
            string? customerEmail,
            int attendees)
        {
            if (facilityId <= 0)
                throw new ValidationException("Facility is required.");
            if (roomId <= 0)
                throw new ValidationException("Room is required.");
            if (attendees <= 0)
                throw new ValidationException("Attendees must be greater than zero.");

            DateRangeValidator.EnsureValidRange(start, end);
            CustomerValidator.ValidateCustomer(customerName, customerEmail);
        }

        /// <summary>
        /// Validates date range when updating a booking's time window.
        /// </summary>
        public static void EnsureUpdateWindowValid(DateTime start, DateTime end)
        {
            DateRangeValidator.EnsureValidRange(start, end);
        }
    }
}

