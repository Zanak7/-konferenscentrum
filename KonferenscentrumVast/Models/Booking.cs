using System;

namespace KonferenscentrumVast.Models
{
    /// <summary>
    /// Represents a facility booking request made by a customer.
    /// Goes through different statuses from Pending to Confirmed/Cancelled/Completed.
    /// You might want to implement some validation.
    /// </summary>
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int FacilityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfParticipants { get; set; }
        public string Notes { get; set; } = string.Empty;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        public Customer Customer { get; set; } = null!;
        public Facility Facility { get; set; } = null!;
        public BookingContract? Contract { get; set; }
    }

    /// <summary>
    /// Defines the different states a booking can be in during its lifecycle
    /// </summary>
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }
}