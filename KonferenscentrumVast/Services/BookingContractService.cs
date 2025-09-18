using System;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Exceptions;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Services
{
    /// <summary>
    /// Manages booking contract lifecycle including creation modification, and status changes.
    /// Handles automatic contract generation (can be created manually if that fails) and maintains snapshot data for legal purposes.
    /// </summary>
    public class BookingContractService
    {
        private readonly IBookingRepository _bookings;
        private readonly IBookingContractRepository _contracts;
        private readonly IFacilityRepository _facilities;
        private readonly ICustomerRepository _customers;
        private readonly ILogger<BookingContractService> _logger;

        public BookingContractService(
            IBookingRepository bookings,
            IBookingContractRepository contracts,
            IFacilityRepository facilities,
            ICustomerRepository customers,
            ILogger<BookingContractService> logger)
        {
            _bookings = bookings;
            _contracts = contracts;
            _facilities = facilities;
            _customers = customers;
            _logger = logger;
        }

        /// <summary>
        /// Creates a basic contract for an existing booking with default terms.
        /// Generates unique contract number and captures snapshot data from related entities.
        /// </summary>
        public async Task<BookingContract> CreateBasicForBookingAsync(int bookingId, string? termsOverride = null, DateTime? paymentDue = null)
        {
            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id={bookingId} was not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new ValidationException("Cannot create a contract for a cancelled booking.");

            var existing = await _contracts.GetByBookingIdAsync(bookingId);
            if (existing != null)
                throw new ConflictException($"A contract already exists for booking id={bookingId} (id={existing.Id}).");

            var facility = await _facilities.GetByIdAsync(booking.FacilityId)
                ?? throw new NotFoundException($"Facility with id={booking.FacilityId} was not found.");

            var customer = await _customers.GetByIdAsync(booking.CustomerId)
                ?? throw new NotFoundException($"Customer with id={booking.CustomerId} was not found.");

            var contract = new BookingContract
            {
                BookingId = booking.Id,
                ContractNumber = GenerateContractNumber(booking.Id),
                Version = 1,
                Status = ContractStatus.Draft,
                Terms = string.IsNullOrWhiteSpace(termsOverride) ? BuildDefaultTerms(booking, facility) : termsOverride.Trim(),
                TotalAmount = booking.TotalPrice,
                Currency = "SEK",
                PaymentDueDate = paymentDue ?? booking.StartDate.AddDays(-7),

                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                CustomerEmail = customer.Email,
                FacilityName = facility.Name,

                CreatedDate = DateTime.UtcNow
            };

            contract = await _contracts.CreateAsync(contract);
            _logger.LogInformation("Created basic contract {ContractId} for booking {BookingId}.", contract.Id, booking.Id);
            return contract;
        }

        public async Task<BookingContract> GetByIdAsync(int contractId)
        {
            return await _contracts.GetByIdAsync(contractId)
                ?? throw new NotFoundException($"Contract with id={contractId} was not found.");
        }

        public async Task<BookingContract> GetByBookingIdAsync(int bookingId)
        {
            return await _contracts.GetByBookingIdAsync(bookingId)
                ?? throw new NotFoundException($"Contract for booking id={bookingId} was not found.");
        }

        /// <summary>
        /// Updates contract terms and amounts. Only allowed for Draft/Sent contracts.
        /// Increments version number and updates timestamp on successful modification.
        /// </summary>
        public async Task<BookingContract> PatchAsync(int id, BookingContractPatchDto dto)
        {
            var existing = await _contracts.GetByIdAsync(id)
                ?? throw new NotFoundException($"Contract with id={id} was not found.");

            if (existing.Status is ContractStatus.Signed or ContractStatus.Cancelled)
                throw new ValidationException("Cannot modify a signed or cancelled contract.");

            if (!string.IsNullOrWhiteSpace(dto.Terms))
                existing.Terms = dto.Terms.Trim();

            if (dto.TotalAmount.HasValue)
            {
                if (dto.TotalAmount.Value < 0)
                    throw new ValidationException("Total amount cannot be negative.");
                existing.TotalAmount = dto.TotalAmount.Value;
            }

            if (dto.PaymentDueDate.HasValue)
                existing.PaymentDueDate = dto.PaymentDueDate.Value;

            existing.Version += 1;
            existing.LastUpdated = DateTime.UtcNow;

            var updated = await _contracts.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Contract with id={id} was not found during update.");

            return updated;
        }

        /// <summary>
        /// Marks contract as sent to customer. Requires booking to be confirmed first.
        /// Used to track when contracts have been delivered to customers.
        /// </summary>
        public async Task<BookingContract> MarkSentAsync(int id)
        {
            var existing = await _contracts.GetByIdAsync(id)
                ?? throw new NotFoundException($"Contract with id={id} was not found.");

            var booking = await _bookings.GetByIdAsync(existing.BookingId)
                ?? throw new NotFoundException($"Booking with id={existing.BookingId} was not found.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new ValidationException("Cannot send contract for unconfirmed booking. Confirm the booking first.");

            if (existing.Status == ContractStatus.Cancelled)
                throw new ValidationException("Cannot mark a cancelled contract as sent.");

            existing.Status = ContractStatus.Sent;
            existing.LastUpdated = DateTime.UtcNow;

            var updated = await _contracts.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Contract with id={id} was not found during update.");

            _logger.LogInformation("Marked contract {ContractId} as sent.", updated.Id);
            return updated;
        }

        /// <summary>
        /// Records contract signature with timestamp. Requires confirmed booking.
        /// Once signed, contract cannot be modified and becomes legally binding.
        /// </summary>
        public async Task<BookingContract> MarkSignedAsync(int id, DateTime? signedAt = null)
        {
            var existing = await _contracts.GetByIdAsync(id)
                ?? throw new NotFoundException($"Contract with id={id} was not found.");

            var booking = await _bookings.GetByIdAsync(existing.BookingId)
                ?? throw new NotFoundException($"Booking with id={existing.BookingId} was not found.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new ValidationException("Cannot sign contract for unconfirmed booking. Confirm the booking first.");

            if (existing.Status == ContractStatus.Cancelled)
                throw new ValidationException("Cannot sign a cancelled contract.");

            existing.Status = ContractStatus.Signed;
            existing.SignedAt = signedAt ?? DateTime.UtcNow;
            existing.LastUpdated = DateTime.UtcNow;

            var updated = await _contracts.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Contract with id={id} was not found during update.");

            _logger.LogInformation("Marked contract {ContractId} as signed.", updated.Id);
            return updated;
        }

        public async Task<BookingContract> CancelAsync(int id, string? reason)
        {
            var existing = await _contracts.GetByIdAsync(id)
                ?? throw new NotFoundException($"Contract with id={id} was not found.");

            existing.Status = ContractStatus.Cancelled;
            existing.CancelledAt = DateTime.UtcNow;
            existing.CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            existing.LastUpdated = DateTime.UtcNow;

            var updated = await _contracts.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Contract with id={id} was not found during update.");

            _logger.LogInformation("Cancelled contract {ContractId}.", updated.Id);
            return updated;
        }

        /// <summary>
        /// Generates unique contract number using format: KCV-YYYY-NNNNNN
        /// Where KCV is company prefix, YYYY is current year, NNNNNN is 6-digit booking ID
        /// </summary>
        private static string GenerateContractNumber(int bookingId)
        {
            return $"KCV-{DateTime.UtcNow:yyyy}-{bookingId:D6}";
        }

        /// <summary>
        /// Builds standardized contract terms with booking details and standard policies.
        /// Includes dates, participant count, pricing, and default cancellation terms.
        /// </summary>
        private static string BuildDefaultTerms(Booking booking, Facility facility)
        {
            return
$@"Booking Contract for {facility.Name}
Dates: {booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}
Participants: {booking.NumberOfParticipants}
Total Amount: {booking.TotalPrice:0.##} SEK

General terms:
- Payment due 7 days before start.
- Cancellation policy: 50% within 7 days, 100% within 48 hours.
- Damages and extra services will be invoiced separately.";
        }
    }
}