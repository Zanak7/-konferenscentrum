using System;
using System.ComponentModel.DataAnnotations;

namespace KonferenscentrumVast.Models
{
    /// <summary>
    /// Represents a customer who can make bookings.
    /// Can be either an individual or represent a company.
    /// You might want to implement some validation...
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}