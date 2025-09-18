using System;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Mvc;

namespace KonferenscentrumVast.Controllers
{
    /// <summary>
    /// API controller for managing customers.
    /// Handles customer lifecycle operations with email uniqueness enforcement and booking safety checks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly ICustomerRepository _customers;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            CustomerService customerService,
            ICustomerRepository customers,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _customers = customers;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all customers with booking statistics
        /// </summary>
        /// <returns>List of all customers</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
        {
            var list = await _customers.GetAllAsync();
            return Ok(list.Select(ToDto));
        }

        /// <summary>
        /// Retrieves a specific customer by ID
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Customer details with booking statistics</returns>
        /// <response code="200">Returns the customer</response>
        /// <response code="404">Customer not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
        {
            var entity = await _customers.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Finds a customer by email address (case-insensitive)
        /// </summary>
        /// <param name="email">Customer email address</param>
        /// <returns>Customer details if found</returns>
        /// <response code="200">Returns the customer</response>
        /// <response code="400">Email parameter is required</response>
        /// <response code="404">Customer not found</response>
        [HttpGet("by-email")]
        public async Task<ActionResult<CustomerResponseDto>> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var entity = await _customerService.GetByEmailAsync(email);
            if (entity == null) return NotFound();
            return Ok(ToDto(entity));
        }

        /// <summary>
        /// Creates a new customer with email uniqueness validation
        /// </summary>
        /// <param name="dto">Customer details</param>
        /// <returns>Created customer</returns>
        /// <response code="201">Customer created successfully</response>
        /// <response code="400">Invalid customer data or validation failed</response>
        /// <response code="409">Customer with email already exists</response>
        [HttpPost]
        public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CustomerCreateDto dto)
        {
            var created = await _customerService.CreateAsync(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                dto.CompanyName,
                dto.Address,
                dto.PostalCode,
                dto.City
            );

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        /// <summary>
        /// Updates customer information with email conflict checking
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="dto">Updated customer details</param>
        /// <returns>Updated customer</returns>
        /// <response code="200">Customer updated successfully</response>
        /// <response code="400">Invalid customer data</response>
        /// <response code="404">Customer not found</response>
        /// <response code="409">Email already used by another customer</response>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CustomerResponseDto>> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            var updated = await _customerService.UpdateAsync(
                id,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                dto.CompanyName,
                dto.Address,
                dto.PostalCode,
                dto.City
            );

            return Ok(ToDto(updated));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteAsync(id);
            return NoContent();
        }

        // ------- Mapping helpers -------

        private static CustomerResponseDto ToDto(Customer c)
        {
            var total = c.Bookings?.Count ?? 0;
            var active = c.Bookings?.Count(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) ?? 0;

            return new CustomerResponseDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                Address = c.Address,
                PostalCode = c.PostalCode,
                City = c.City,
                CreatedDate = c.CreatedDate,
                TotalBookings = total,
                ActiveBookings = active
            };
        }
    }
}