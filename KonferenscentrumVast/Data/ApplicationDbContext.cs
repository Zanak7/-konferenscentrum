using System;
using KonferenscentrumVast.Models;
using Microsoft.EntityFrameworkCore;

namespace KonferenscentrumVast.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingContract> BookingContracts { get; set; }
    }
}