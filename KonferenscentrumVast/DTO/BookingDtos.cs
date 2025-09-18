using System;
namespace KonferenscentrumVast.DTO
{
    public sealed class BookingCreateDto
    {
        public int CustomerId { get; set; }
        public int FacilityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfParticipants { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class BookingRescheduleDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public sealed class BookingCancelDto
    {
        public string? Reason { get; set; }
    }

    public sealed class BookingResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int FacilityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfParticipants { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        // convenience (avoid deep nesting...)
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? FacilityName { get; set; }
        public int? ContractId { get; set; }
    }
}