using System;
namespace KonferenscentrumVast.DTO
{
    public class BookingContractCreateDto
    {
        public string? Terms { get; set; }
        public DateTime? PaymentDueDate { get; set; }
    }

    
    public class BookingContractUpdateDto
    {
        public string? Terms { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? PaymentDueDate { get; set; }
    }

    public class BookingContractPatchDto
    {
        public string? Terms { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? PaymentDueDate { get; set; }
    }

    public class BookingContractResponseDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Status { get; set; } = string.Empty;

        public string Terms { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "SEK";
        public DateTime? PaymentDueDate { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
    }
}

