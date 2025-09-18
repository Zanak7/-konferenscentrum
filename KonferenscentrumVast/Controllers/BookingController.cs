using System;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Mvc;

namespace KonferenscentrumVast.Controllers
{
    /// <summary>
    /// API controller for managing facility bookings.
    /// Handles booking creation, confirmation, rescheduling, and cancellation with availability checking.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly IBookingRepository _bookings;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            BookingService bookingService,
            IBookingRepository bookings,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _bookings = bookings;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specific booking by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details including customer, facility, and contract information</returns>
        /// <response code="200">Returns the booking</response>
        /// <response code="404">Booking not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingResponseDto>> GetById(int id)
        {
            var booking = await _bookings.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(ToDto(booking));
        }

        /// <summary>
        /// Retrieves all bookings in the system
        /// </summary>
        /// <returns>List of all bookings</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAll()
        {
            var data = await _bookings.GetAllAsync();
            return Ok(data.Select(ToDto));
        }

        /// <summary>
        /// Retrieves bookings filtered by customer, facility, or date range
        /// </summary>
        /// <param name="customerId">Filter by customer ID</param>
        /// <param name="facilityId">Filter by facility ID</param>
        /// <param name="from">Start date for date range filter</param>
        /// <param name="to">End date for date range filter</param>
        /// <returns>Filtered list of bookings</returns>
        /// <response code="200">Returns filtered bookings</response>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetFiltered(
            [FromQuery] int? customerId,
            [FromQuery] int? facilityId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            IEnumerable<Booking> data;

            if (customerId.HasValue)
                data = await _bookings.GetByCustomerIdAsync(customerId.Value);
            else if (facilityId.HasValue)
                data = await _bookings.GetByFacilityIdAsync(facilityId.Value);
            else if (from.HasValue && to.HasValue)
                data = await _bookings.GetByDateRangeAsync(from.Value, to.Value);
            else
                data = await _bookings.GetAllAsync();

            return Ok(data.Select(ToDto));
        }

        /// <summary>
        /// Creates a new booking with availability checking and automatic contract generation
        /// </summary>
        /// <param name="request">Booking details</param>
        /// <returns>Created booking</returns>
        /// <response code="201">Booking created successfully</response>
        /// <response code="400">Invalid booking data or validation failed</response>
        /// <response code="404">Customer or facility not found</response>
        /// <response code="409">Booking conflict - facility unavailable for specified dates</response>
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> Create([FromBody] BookingCreateDto request)
        {
            var booking = await _bookingService.CreateBookingAsync(
                request.CustomerId,
                request.FacilityId,
                request.StartDate,
                request.EndDate,
                request.NumberOfParticipants,
                request.Notes);

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, ToDto(booking));
        }

        /// <summary>
        /// Confirms a pending booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Confirmed booking</returns>
        /// <response code="200">Booking confirmed</response>
        /// <response code="400">Cannot confirm cancelled or past booking</response>
        /// <response code="404">Booking not found</response>
        [HttpPost("{id:int}/confirm")]
        public async Task<ActionResult<BookingResponseDto>> Confirm(int id)
        {
            var updated = await _bookingService.ConfirmBookingAsync(id);
            return Ok(ToDto(updated));
        }

        /// <summary>
        /// Changes booking dates with availability checking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">New start and end dates</param>
        /// <returns>Rescheduled booking with updated pricing</returns>
        /// <response code="200">Booking rescheduled</response>
        /// <response code="400">Invalid dates or cancelled booking</response>
        /// <response code="404">Booking not found</response>
        /// <response code="409">New dates conflict with existing bookings</response>
        [HttpPost("{id:int}/reschedule")]
        public async Task<ActionResult<BookingResponseDto>> Reschedule(int id, [FromBody] BookingRescheduleDto request)
        {
            var updated = await _bookingService.RescheduleBookingAsync(id, request.StartDate, request.EndDate);
            return Ok(ToDto(updated));
        }

        /// <summary>
        /// Cancels a booking with optional reason (idempotent operation)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Optional cancellation details</param>
        /// <returns>No content</returns>
        /// <response code="204">Booking cancelled successfully</response>
        /// <response code="404">Booking not found</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id, [FromBody] BookingCancelDto? request)
        {
            await _bookingService.CancelBookingAsync(id, request?.Reason);
            return NoContent();
        }

        // ------- mapping helpers-------
        /// <summary>
        /// Converts domain model to response DTO, flattening related entity data for easier consumption
        /// </summary>
        private static BookingResponseDto ToDto(Booking b)
        {
            return new BookingResponseDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                FacilityId = b.FacilityId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                NumberOfParticipants = b.NumberOfParticipants,
                Notes = b.Notes ?? string.Empty,
                Status = b.Status.ToString(),
                TotalPrice = b.TotalPrice,
                CreatedDate = b.CreatedDate,
                ConfirmedDate = b.ConfirmedDate,
                CancelledDate = b.CancelledDate,
                CustomerName = b.Customer != null ? $"{b.Customer.FirstName} {b.Customer.LastName}".Trim() : null,
                CustomerEmail = b.Customer?.Email,
                FacilityName = b.Facility?.Name,
                ContractId = b.Contract?.Id
            };
        }
    }
}