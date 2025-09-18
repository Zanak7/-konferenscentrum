using System;

namespace KonferenscentrumVast.Models
{
    /// <summary>
    /// Represents a legal contract for a booking. 
    /// Automatically created when a booking is made, but can also be created manually if auto-creation fails.
    /// Contains snapshot data to preserve contract details even if related entities change.
    /// You might want to add some validation...
    /// </summary>
    public class BookingContract
    {
        public int Id { get; set; }
        public int BookingId { get; set; }

        public string ContractNumber { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public string Terms { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "SEK";

        public DateTime? PaymentDueDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }

        // snapshot fields
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;

        public Booking Booking { get; set; } = null!;
    }

    /// <summary>
    /// Defines the different states a contract can be in during its lifecycle
    /// </summary>
    public enum ContractStatus
    {
        Draft = 0,
        Sent = 1,
        Signed = 2,
        Cancelled = 3
    }
}