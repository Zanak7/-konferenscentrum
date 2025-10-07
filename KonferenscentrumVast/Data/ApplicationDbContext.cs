using System;
using KonferenscentrumVast.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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

    namespace KonferenscentrumVast.Data
    {
        public class AuthDbContext : IdentityDbContext<IdentityUser>
        {
            public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
            {
            }
        }
    }
}