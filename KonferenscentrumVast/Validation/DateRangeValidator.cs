using System;
using KonferenscentrumVast.Exceptions;

namespace KonferenscentrumVast.Validation
{
    /// <summary>
    /// Validates date ranges and ensures dates meet business requirements.
    /// Prevents booking in the past and ensures logical date sequences.
    /// </summary>
    public static class DateRangeValidator
    {
        public static bool IsDateInPast(DateTime date) => date.Date < DateTime.Now.Date;

        public static bool IsValidRange(DateTime start, DateTime end) =>
            start.Date >= DateTime.Now.Date && end > start;

        public static void EnsureFutureDate(DateTime date, string fieldName)
        {
            if (IsDateInPast(date))
                throw new ValidationException($"{fieldName} must be in the future.");
        }

        public static void EnsureValidRange(DateTime start, DateTime end)
        {
            if (!IsValidRange(start, end))
                throw new ValidationException("Invalid date range: start must be today or later and end must be after start.");
        }
    }
}

