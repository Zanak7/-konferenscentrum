using System;

namespace KonferenscentrumVast.Models
{
    /// <summary>
    /// Represents a bookable conference facility with pricing and capacity information.
    /// Can be activated/deactivated without deleting historical booking data.
    /// You might want to implement some validation...
    /// </summary>
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; } = true;
        public string ImagePaths { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}