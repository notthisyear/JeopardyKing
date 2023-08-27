using System.Globalization;
using System.Windows.Controls;

namespace JeopardyKing.WpfComponents.ValidationRules
{
    public class QuestionValueValidationRule : ValidationRule
    {
        public double Min { get; set; } = 0.0;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string s)
                return new(false, "Could not parse input as string");

            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                return new(false, "Could not parse input as valid number");

            if (s.Length < Min)
                return new ValidationResult(false, $"Input must be larger than {Min}");
            return ValidationResult.ValidResult;
        }
    }
}
