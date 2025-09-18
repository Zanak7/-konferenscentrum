using System;
namespace KonferenscentrumVast.DTO
{
    public class FacilityCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FacilityUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FacilitySetActiveDto
    {
        public bool IsActive { get; set; }
    }

    public class FacilityResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

