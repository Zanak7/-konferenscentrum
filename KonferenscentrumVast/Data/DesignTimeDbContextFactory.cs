using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KonferenscentrumVast.Data
{
    /// <summary>
    /// Gör att 'dotnet ef' kan skapa ApplicationDbContext utan att starta hela hosten (och utan Key Vault).
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Bygg konfiguration från appsettings + user-secrets (ingen Key Vault här)
            var basePath = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets(typeof(Program).Assembly, optional: true) // funkar pga du körde 'user-secrets init'
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("DefaultConnection");
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(cs) // samma provider som i din app
                .Options;

            return new ApplicationDbContext(opts);
        }
    }
}