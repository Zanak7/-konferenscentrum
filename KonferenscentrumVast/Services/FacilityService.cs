using System;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Exceptions;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace KonferenscentrumVast.Services
{
    /// <summary>
    /// Manages conference facility operations including creation, updates, and activation status.
    /// Handles facility data validation and soft-delete through activation toggles.
    /// </summary>
    public sealed class FacilityService
    {
        private readonly IFacilityRepository _facilities;
        private readonly ILogger<FacilityService> _logger;
        private readonly BlobServiceClient _blob;
        private readonly string _imagesContainer;

        public FacilityService(
            IFacilityRepository facilities,
            ILogger<FacilityService> logger,
            BlobServiceClient blob,
            IConfiguration cfg)
        {
            _facilities = facilities;
            _logger = logger;
            _blob = blob;
            _imagesContainer = cfg["AzureStorage:ImagesContainer"]
                ?? throw new InvalidOperationException("Missing AzureStorage:ImagesContainer");
        }

        public Task<IEnumerable<Facility>> GetAllAsync() => _facilities.GetAllAsync();

        public Task<IEnumerable<Facility>> GetActiveAsync() => _facilities.GetActiveAsync();

        public async Task<Facility> GetByIdAsync(int id)
        {
            return await _facilities.GetByIdAsync(id)
                ?? throw new NotFoundException($"Facility with id={id} was not found.");
        }

        public async Task<Facility> CreateAsync(
            string name,
            string description,
            string address,
            string postalCode,
            string city,
            int maxCapacity,
            decimal pricePerDay,
            bool isActive)
        {
            EnsureFacilityFields(name, address, postalCode, city, maxCapacity, pricePerDay);

            var facility = new Facility
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Address = address.Trim(),
                PostalCode = postalCode.Trim(),
                City = city.Trim(),
                MaxCapacity = maxCapacity,
                PricePerDay = pricePerDay,
                IsActive = isActive,
                ImagePaths = string.Empty,
                CreatedDate = DateTime.UtcNow
            };

            facility = await _facilities.CreateAsync(facility);
            _logger.LogInformation("Created facility {FacilityId} ({Name}).", facility.Id, facility.Name);

            return facility;
        }

        public async Task<Facility> UpdateAsync(
            int id,
            string name,
            string description,
            string address,
            string postalCode,
            string city,
            int maxCapacity,
            decimal pricePerDay,
            bool isActive)
        {
            EnsureFacilityFields(name, address, postalCode, city, maxCapacity, pricePerDay);

            var existing = await _facilities.GetByIdAsync(id)
                ?? throw new NotFoundException($"Facility with id={id} was not found.");

            existing.Name = name.Trim();
            existing.Description = description?.Trim() ?? string.Empty;
            existing.Address = address.Trim();
            existing.PostalCode = postalCode.Trim();
            existing.City = city.Trim();
            existing.MaxCapacity = maxCapacity;
            existing.PricePerDay = pricePerDay;
            existing.IsActive = isActive;

            var updated = await _facilities.UpdateAsync(existing.Id, existing)
                ?? throw new NotFoundException($"Facility with id={id} was not found during update.");

            _logger.LogInformation("Updated facility {FacilityId}.", updated.Id);
            return updated;
        }

        public async Task DeleteAsync(int id)
        {
            var facility = await _facilities.GetByIdAsync(id)
                ?? throw new NotFoundException($"Facility with id={id} was not found.");


            var removed = await _facilities.DeleteAsync(id);
            if (!removed)
                throw new NotFoundException($"Facility with id={id} was not found during delete.");

            _logger.LogInformation("Deleted facility {FacilityId}.", id);
        }

        /// <summary>
        /// Toggles facility active status for soft delete functionality.
        /// Inactive facilities cannot be booked but preserve historical booking data.
        /// </summary>
        public async Task<Facility> SetActiveAsync(int facilityId, bool isActive)
        {
            var facility = await _facilities.GetByIdAsync(facilityId)
                ?? throw new NotFoundException($"Facility with id={facilityId} was not found.");

            facility.IsActive = isActive;

            var updated = await _facilities.UpdateAsync(facility.Id, facility)
                ?? throw new NotFoundException($"Facility with id={facility.Id} was not found during activation update.");

            _logger.LogInformation("Set facility {FacilityId} active={Active}.", facility.Id, isActive);
            return updated;
        }

         /// <summary>
        /// Handles image file uploads by validating input, checking file type and size,
        /// ensuring the facility exists, and then uploading the file to Azure Blob Storage.
        /// Returns information about the uploaded file, including its path, name, and size.
        /// </summary>

        public async Task<FileUploadResultDto> UploadImageAsync(IFormFile file, int facilityId)
        {
            //Validations for the id and the files
            if (facilityId <= 0)
                throw new ValidationException("Invalid id.");

            if (file == null || file.Length == 0)
                throw new ValidationException("File is missing or empty.");

            const long MaxBytes = 10 * 1024 * 1024; // 10 MB
            if (file.Length > MaxBytes)
                throw new ValidationException("File is too large.");

            //Gets the facility with the id and checks if it exists
            var facility = await _facilities.GetByIdAsync(facilityId);

            if (facility is null) throw new NotFoundException("Facility not found");

            //If the ContentType/extension is null, use an empty string to prevent the app from crashing
            // Convert them to small letters
            var ct = (file.ContentType ?? "").ToLowerInvariant();
            var ext = (Path.GetExtension(file.FileName) ?? "").ToLowerInvariant();

            //Check if the file is JPG or PNG
            var okImage =
                (ct == "image/jpeg" && (ext == ".jpg" || ext == ".jpeg")) ||
                (ct == "image/png" && ext == ".png");

            if (!okImage) throw new ValidationException("Only JPG or PNG is allowed.");

            //Gets only the filename and creates a unique blob name under this id
            var safeName = Path.GetFileName(file.FileName);
            var blobName = $"{facilityId}/{Guid.NewGuid()}_{safeName}";

            //Gets the container and blob with the connection string
            var container = _blob.GetBlobContainerClient(_imagesContainer);
            var blob = container.GetBlobClient(blobName);

            //Open the read stream and upload the blob
            using var s = file.OpenReadStream();

            await blob.UploadAsync(
                s,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType ?? "application/octet-stream",
                    },
                }
            );

            _logger.LogInformation(
                "Uploaded image file {FileName} for facility id {FacilityId} to container {Container} as {BlobName} ({SizedBytes} bytes).",
                safeName, facilityId, _imagesContainer, blobName, file.Length);

            //Returns information about the uploaded file, including its path, name and size.
            return new FileUploadResultDto
            {
                BlobPath = blobName,
                FileName = safeName,
                Size = file.Length,
            };
        }

        /// <summary>
        /// Validates required facility fields and business constraints.
        /// Ensures all mandatory data is present and values are within acceptable ranges.
        /// </summary>
        private static void EnsureFacilityFields(
            string name,
            string address,
            string postalCode,
            string city,
            int maxCapacity,
            decimal pricePerDay)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Facility name is required.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ValidationException("Address is required.");
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ValidationException("Postal code is required.");
            if (string.IsNullOrWhiteSpace(city))
                throw new ValidationException("City is required.");
            if (maxCapacity <= 0)
                throw new ValidationException("Max capacity must be greater than zero.");
            if (pricePerDay < 0)
                throw new ValidationException("Price per day cannot be negative.");
        }
    }
}