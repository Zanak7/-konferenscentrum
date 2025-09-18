using System;
using KonferenscentrumVast.Models;

namespace KonferenscentrumVast.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for managing facility data operations.
    /// Includes active facility filtering for booking availability.
    /// </summary>
    public interface IFacilityRepository
    {
        Task<IEnumerable<Facility>> GetAllAsync();
        Task<IEnumerable<Facility>> GetActiveAsync();
        Task<Facility?> GetByIdAsync(int id);
        Task<Facility> CreateAsync(Facility facility);
        Task<Facility?> UpdateAsync(int id, Facility facility);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
