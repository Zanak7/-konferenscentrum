using System;
using System.Diagnostics.Contracts;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Mvc;

namespace KonferenscentrumVast.Controllers
{
    /// <summary>
    /// API controller for managing booking contracts.
    /// Handles contract lifecycle from creation through signing or cancellation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingContractController : ControllerBase
    {
        private readonly BookingContractService _service;
        private readonly IBookingContractRepository _contracts;
        private readonly ILogger<BookingContractController> _logger;

        public BookingContractController(BookingContractService service, IBookingContractRepository contracts, ILogger<BookingContractController> logger)
        {
            _service = service;
            _logger = logger;
            _contracts = contracts;
        }

        /// <summary>
        /// Retrieves a specific contract by ID
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Contract details</returns>
        /// <response code="200">Returns the contract</response>
        /// <response code="404">Contract not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Retrieves all contracts in the system
        /// </summary>
        /// <returns>List of all contracts</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingContractResponseDto>>> GetAll()
        {
            var data = await _contracts.GetAllAsync();
            return Ok(data.Select(ToDto));
        }

        /// <summary>
        /// Retrieves the contract associated with a specific booking
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>Contract for the booking</returns>
        /// <response code="200">Returns the contract</response>
        /// <response code="404">Contract not found for booking</response>
        [HttpGet("booking/{bookingId:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> GetByBookingId(int bookingId)
        {
            var entity = await _service.GetByBookingIdAsync(bookingId);
            return Ok(ToDto(entity));
        }

        /// create a basic contract for a booking (if not auto-created)
        // <summary>
        /// Manually creates a basic contract for a booking if auto-creation failed
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <param name="dto">Optional contract terms and payment due date</param>
        /// <returns>Created contract</returns>
        /// <response code="201">Contract created successfully</response>
        /// <response code="400">Invalid booking or contract data</response>
        /// <response code="409">Contract already exists for booking</response>
        [HttpPost("booking/{bookingId:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> CreateContract(int bookingId, [FromBody] BookingContractCreateDto dto)
        {
            var entity = await _service.CreateBasicForBookingAsync(bookingId, dto.Terms, dto.PaymentDueDate);
            return CreatedAtAction(nameof(GetByBookingId), new { bookingId = entity.BookingId }, ToDto(entity));
        }

        /// <summary>
        /// Updates contract terms, amount, or payment due date
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="dto">Contract updates</param>
        /// <returns>Updated contract</returns>
        /// <response code="200">Contract updated successfully</response>
        /// <response code="400">Cannot modify signed or cancelled contract</response>
        /// <response code="404">Contract not found</response>
        [HttpPatch("{id:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> Patch(int id, [FromBody] BookingContractPatchDto dto)
        {
            var entity = await _service.PatchAsync(id, dto);
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Marks contract as sent to customer
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Updated contract with Sent status</returns>
        /// <response code="200">Contract marked as sent</response>
        /// <response code="400">Booking must be confirmed first</response>
        /// <response code="404">Contract not found</response>
        [HttpPost("{id:int}/send")]
        public async Task<ActionResult<BookingContractResponseDto>> MarkSent(int id)
        {
            var entity = await _service.MarkSentAsync(id);
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Records contract signature
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Updated contract with Signed status</returns>
        /// <response code="200">Contract marked as signed</response>
        /// <response code="400">Booking must be confirmed first</response>
        /// <response code="404">Contract not found</response>
        [HttpPost("{id:int}/sign")]
        public async Task<ActionResult<BookingContractResponseDto>> MarkSigned(int id)
        {
            var entity = await _service.MarkSignedAsync(id);
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Cancels a contract with optional reason
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="reason">Cancellation reason</param>
        /// <returns>Cancelled contract</returns>
        /// <response code="200">Contract cancelled</response>
        /// <response code="404">Contract not found</response>

        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<BookingContractResponseDto>> Cancel(int id, [FromBody] string? reason)
        {
            var entity = await _service.CancelAsync(id, reason);
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Converts domain model to response DTO with flattened data for API consumers
        /// </summary>
        private static BookingContractResponseDto ToDto(BookingContract c)
        {
            return new BookingContractResponseDto
            {
                Id = c.Id,
                BookingId = c.BookingId,
                ContractNumber = c.ContractNumber,
                Version = c.Version,
                Status = c.Status.ToString(),
                Terms = c.Terms,
                TotalAmount = c.TotalAmount,
                Currency = c.Currency,
                PaymentDueDate = c.PaymentDueDate,
                CustomerName = c.CustomerName,
                CustomerEmail = c.CustomerEmail,
                FacilityName = c.FacilityName,
                CreatedDate = c.CreatedDate,
                LastUpdated = c.LastUpdated,
                SignedAt = c.SignedAt,
                CancelledAt = c.CancelledAt,
                CancelReason = c.CancelReason
            };
        }
    }
}