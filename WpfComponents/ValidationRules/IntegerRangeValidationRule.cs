using System.Globalization;
using System.Windows.Controls;

namespace JeopardyKing.WpfComponents.ValidationRules
{
    public class IntegerRangeValidationRule : ValidationRule
    {
        public int MinInclusive { get; set; } = 0;

        public int MaxInclusive { get; set; } = 100;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string s || !int.TryParse(s, out var i))
                return new(false, "Could not parse input as integer");

            if (i < MinInclusive || i > MaxInclusive)
                return new ValidationResult(false, $"Input must be in the range {MinInclusive} - {MaxInclusive}");
            return ValidationResult.ValidResult;
        }
    }
}
