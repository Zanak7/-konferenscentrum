using System;
namespace KonferenscentrumVast.Exceptions
{
    /// <summary>
    /// Exception thrown when input data fails validation rules.
    /// Maps to HTTP 400 Bad Request status code in the API response.
    /// Used for business rule violations, invalid formats, and constraint failures.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}