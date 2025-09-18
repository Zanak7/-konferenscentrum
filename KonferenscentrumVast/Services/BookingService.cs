using System;
using KonferenscentrumVast.Exceptions;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Validation;

namespace KonferenscentrumVast.Services
{
    /// <summary>
    /// Manages the complete booking lifecycle from creation to completion.
    /// Handles availability checking, pricing calculations, and automatic contract generation.
    /// </summary>
    public class BookingService
    {
        private readonly IBookingRepository _bookings;
        private readonly ICustomerRepository _customers;
        private readonly IFacilityRepository _facilities;
        private readonly ILogger<BookingService> _logger;
        private readonly BookingContractService _contractService;

        public BookingService(IBookingRepository bookings, ICustomerRepository customers, IFacilityRepository facilities, ILogger<BookingService> logger, BookingContractService contractService)
        {
            _bookings = bookings;
            _customers = customers;
            _facilities = facilities;
            _logger = logger;
            _contractService = contractService;
        }

        /// <summary>
        /// Creates a new booking with validation and conflict checking.
        /// Automatically calculates pricing and attempts to generate contract.
        /// Verifies facility availability and capacity constraints.
        /// </summary>
        public async Task<Booking> CreateBookingAsync(
            int customerId,
            int facilityId,
            DateTime startDate,
            DateTime endDate,
            int numberOfParticipants,
            string? notes)
        {
            if (numberOfParticipants <= 0)
                throw new ValidationException("Number of participants must be greater than zero.");

            DateRangeValidator.EnsureValidRange(startDate, endDate);

            var customer = await _customers.GetByIdAsync(customerId)
                ?? throw new NotFoundException($"Customer with id={customerId} was not found.");

            var facility = await _facilities.GetByIdAsync(facilityId)
                ?? throw new NotFoundException($"Facility with id={facilityId} was not found.");

            if (!facility.IsActive)
                throw new ValidationException("Facility is not active and cannot be booked.");

            if (numberOfParticipants > facility.MaxCapacity)
                throw new ValidationException($"Participants exceed facility capacity ({facility.MaxCapacity}).");

            var statuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed };
            var hasConflict = await _bookings.HasOverlapAsync(facilityId, startDate, endDate, statuses);

            if (hasConflict)
                throw new ConflictException($"Booking conflict for facility {facilityId} between {startDate:yyyy-MM-dd HH:mm} and {endDate:yyyy-MM-dd HH:mm}.");

            var totalPrice = CalculateTotalPrice(facility, startDate, endDate);

            var booking = new Booking
            {
                CustomerId = customer.Id,
                FacilityId = facility.Id,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfParticipants = numberOfParticipants,
                Notes = notes?.Trim() ?? string.Empty,
                Status = BookingStatus.Pending,
                TotalPrice = totalPrice,
                CreatedDate = DateTime.UtcNow
            };

            booking = await _bookings.CreateAsync(booking);

            await _contractService.CreateBasicForBookingAsync(booking.Id);

            _logger.LogInformation("Created booking {BookingId} for facility {FacilityId} and customer {CustomerId}.",
                booking.Id, booking.FacilityId, booking.CustomerId);

            return booking;
        }

        public async Task<Booking> ConfirmBookingAsync(int bookingId)
        {
            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id={bookingId} was not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new ValidationException("Cannot confirm a cancelled booking.");

            if (DateRangeValidator.IsDateInPast(booking.StartDate))
                throw new ValidationException("Cannot confirm a booking that starts in the past.");

            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedDate = DateTime.UtcNow;

            var updated = await _bookings.UpdateAsync(booking.Id, booking)
                ?? throw new NotFoundException($"Booking with id={booking.Id} was not found during update.");

            _logger.LogInformation("Confirmed booking {BookingId}.", updated.Id);
            return updated;
        }

        /// <summary>
        /// Changes booking dates with availability checking and price recalculation.
        /// Validates new dates don't conflict with other bookings (excluding current booking).
        /// </summary>
        public async Task<Booking> RescheduleBookingAsync(int bookingId, DateTime newStart, DateTime newEnd)
        {
            DateRangeValidator.EnsureValidRange(newStart, newEnd);

            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id={bookingId} was not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new ValidationException("Cannot reschedule a cancelled booking.");

            var facility = await _facilities.GetByIdAsync(booking.FacilityId)
                ?? throw new NotFoundException($"Facility with id={booking.FacilityId} was not found.");

            if (!facility.IsActive)
                throw new ValidationException("Facility is not active and cannot be booked.");

            var statuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed };
            var hasConflict = await _bookings.HasOverlapExcludingAsync(booking.Id, booking.FacilityId, newStart, newEnd, statuses);

            if (hasConflict)
                throw new ConflictException($"Booking conflict for facility {booking.FacilityId} between {newStart:yyyy-MM-dd HH:mm} and {newEnd:yyyy-MM-dd HH:mm}.");

            booking.StartDate = newStart;
            booking.EndDate = newEnd;
            booking.TotalPrice = CalculateTotalPrice(facility, newStart, newEnd);

            var updated = await _bookings.UpdateAsync(booking.Id, booking)
                ?? throw new NotFoundException($"Booking with id={booking.Id} was not found during update.");

            _logger.LogInformation("Rescheduled booking {BookingId} to {Start} - {End}.", updated.Id, newStart, newEnd);
            return updated;
        }

        public async Task CancelBookingAsync(int bookingId, string? reason)
        {
            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id={bookingId} was not found.");

            if (booking.Status == BookingStatus.Cancelled)
                return; // idempotent 
            // means if someone tries to cancel a booking that's already cancelled,
            // the method just returns successfully instead of throwing an error
            // this is is a REST API best practice - DELETE and PUT operations should typically be idempotent
            // so clients can safely retry failed requests..

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledDate = DateTime.UtcNow;
            var note = string.IsNullOrWhiteSpace(reason) ? "" : $" Cancellation reason: {reason.Trim()}";
            booking.Notes = (booking.Notes ?? string.Empty) + note;

            var updated = await _bookings.UpdateAsync(booking.Id, booking)
                ?? throw new NotFoundException($"Booking with id={booking.Id} was not found during update.");

            _logger.LogInformation("Cancelled booking {BookingId}.", updated.Id);
        }

        /// <summary>
        /// Calculates total price based on facility's daily rate and booking duration.
        /// Ensures minimum of 1 day billing even for same-day bookings.
        /// </summary>
        private static decimal CalculateTotalPrice(Facility facility, DateTime start, DateTime end)
        {
            var days = Math.Max(1, (end.Date - start.Date).Days);
            return facility.PricePerDay * days;
        }
    }
}
