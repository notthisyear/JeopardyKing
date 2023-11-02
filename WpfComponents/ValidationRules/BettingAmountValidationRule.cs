using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace JeopardyKing.WpfComponents.ValidationRules
{
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new PropertyMetadata(null));
    }

    public class Wrapper : DependencyObject
    {
        public decimal MaxInclusive
        {
            get { return (decimal)GetValue(MaxInclusiveProperty); }
            set { SetValue(MaxInclusiveProperty, value); }
        }

        public static readonly DependencyProperty MaxInclusiveProperty = DependencyProperty.Register(
            nameof(MaxInclusive),
            typeof(decimal),
            typeof(Wrapper),
            new FrameworkPropertyMetadata(decimal.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
    }

    public class BettingAmountValidationRule : ValidationRule
    {
        public Wrapper Wrapper { get; set; } = new();

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string s || !int.TryParse(s, out var i))
                return new(false, "Could not parse input as integer");

            if (Wrapper.MaxInclusive < 0)
            {
                if (i == 0)
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "Player cannot bet - has no money");
            }
            else
            {
                if (i < 0 || i > Wrapper.MaxInclusive)
                    return new ValidationResult(false, $"Bet amount must be in the range 0 - {Wrapper.MaxInclusive}");
                return ValidationResult.ValidResult;
            }
        }
    }
}
