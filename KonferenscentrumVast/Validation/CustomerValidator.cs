using System;
using KonferenscentrumVast.Exceptions;

namespace KonferenscentrumVast.Validation
{
    /// <summary>
    /// Validates customer information including name and email format.
    /// </summary>
    public static class CustomerValidator
    {
        /// <summary>
        /// Ensures customer has a valid name and optionally validates email if provided.
        /// </summary>
        public static void ValidateCustomer(string name, string? email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Customer name is required.");

            if (!string.IsNullOrWhiteSpace(email) && !EmailValidator.IsValid(email))
                throw new ValidationException("Invalid customer email address.");
        }
    }
}

