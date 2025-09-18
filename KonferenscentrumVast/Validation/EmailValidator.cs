using System;
using System.Text.RegularExpressions;

namespace KonferenscentrumVast.Validation
{
    /// <summary>
    /// Validates email addresses using regex pattern based on RFC 5322 standards.
    /// Handles null/empty emails and enforces reasonable length limits.
    /// </summary>
    public static class EmailValidator
    {
        /// <summary>
        /// Compiled regex pattern for email validation based on RFC 5322 (Internet Message Format)
        /// </summary>
        private static readonly Regex Pattern = new(
            @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool IsValid(string? email) =>
            !string.IsNullOrWhiteSpace(email) &&
            email.Length <= 254 &&
            Pattern.IsMatch(email.Trim());
    }
}