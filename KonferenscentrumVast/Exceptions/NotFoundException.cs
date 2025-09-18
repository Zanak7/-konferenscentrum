using System;
namespace KonferenscentrumVast.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource cannot be found.
    /// Maps to HTTP 404 Not Found status code in the API response.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        /// <summary>
        /// Creates a standardized not found exception with consistent messaging format.
        /// Handles both generic resources and resources with specific identifiers.
        /// </summary>
        public static NotFoundException ForResource(string resource, object? id = null) =>
            new NotFoundException(id is null ? $"{resource} was not found." : $"{resource} with id={id} was not found.");
    }
}
