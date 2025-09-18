using System;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Mvc;

namespace KonferenscentrumVast.Controllers
{
    /// <summary>
    /// API controller for managing conference facilities.
    /// Provides both administrative facility management and customer-facing active facility queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FacilityController : ControllerBase
    {
        private readonly FacilityService _service;
        private readonly ILogger<FacilityController> _logger;

        public FacilityController(FacilityService service, ILogger<FacilityController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all facilities (including inactive ones for administrative purposes)
        /// </summary>
        /// <returns>List of all facilities</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacilityResponseDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list.Select(ToDto));
        }


        /// <summary>
        /// Retrieves only active facilities available for booking
        /// </summary>
        /// <returns>List of active facilities</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FacilityResponseDto>>> GetActive()
        {
            var list = await _service.GetActiveAsync();
            return Ok(list.Select(ToDto));
        }

        /// <summary>
        /// Retrieves a specific facility by ID
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <returns>Facility details</returns>
        /// <response code="200">Returns the facility</response>
        /// <response code="404">Facility not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FacilityResponseDto>> GetById(int id)
        {
            var facility = await _service.GetByIdAsync(id);
            return Ok(ToDto(facility));
        }

        /// <summary>
        /// Creates a new facility with validation
        /// </summary>
        /// <param name="dto">Facility details</param>
        /// <returns>Created facility</returns>
        /// <response code="201">Facility created successfully</response>
        /// <response code="400">Invalid facility data or validation failed</response>
        [HttpPost]
        public async Task<ActionResult<FacilityResponseDto>> Create([FromBody] FacilityCreateDto dto)
        {
            var facility = await _service.CreateAsync(
                dto.Name, dto.Description, dto.Address, dto.PostalCode, dto.City,
                dto.MaxCapacity, dto.PricePerDay, dto.IsActive);

            return CreatedAtAction(nameof(GetById), new { id = facility.Id }, ToDto(facility));
        }

        /// <summary>
        /// Updates facility information
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <param name="dto">Updated facility details</param>
        /// <returns>Updated facility</returns>
        /// <response code="200">Facility updated successfully</response>
        /// <response code="400">Invalid facility data</response>
        /// <response code="404">Facility not found</response>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<FacilityResponseDto>> Update(int id, [FromBody] FacilityUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(
                id, dto.Name, dto.Description, dto.Address, dto.PostalCode, dto.City,
                dto.MaxCapacity, dto.PricePerDay, dto.IsActive);

            return Ok(ToDto(updated));
        }

        /// <summary>
        /// Permanently deletes a facility
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <returns>No content</returns>
        /// <response code="204">Facility deleted successfully</response>
        /// <response code="404">Facility not found</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Toggles facility active status (soft delete functionality)
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <param name="dto">Active status</param>
        /// <returns>Updated facility</returns>
        /// <response code="200">Facility status updated</response>
        /// <response code="404">Facility not found</response>
        /// <remarks>
        /// Use this instead of DELETE to preserve historical booking data.
        /// Inactive facilities cannot be booked but maintain data integrity.
        /// </remarks>
        [HttpPatch("{id:int}/active")]
        public async Task<ActionResult<FacilityResponseDto>> SetActive(int id, [FromBody] FacilitySetActiveDto dto)
        {
            var updated = await _service.SetActiveAsync(id, dto.IsActive);
            return Ok(ToDto(updated));
        }

        private static FacilityResponseDto ToDto(Facility f)
        {
            return new FacilityResponseDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Address = f.Address,
                PostalCode = f.PostalCode,
                City = f.City,
                MaxCapacity = f.MaxCapacity,
                PricePerDay = f.PricePerDay,
                IsActive = f.IsActive,
                CreatedDate = f.CreatedDate
            };
        }
    }
}