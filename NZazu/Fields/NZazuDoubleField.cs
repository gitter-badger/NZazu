using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NZazu.Fields
{
    internal class NZazuDoubleField : NZazuField<double?>
    {
        #region converter

        internal class DoubleToStringConverter : IValueConverter
        {
            private readonly string _format;
            private readonly CultureInfo _culture;
            private string _attachDot = string.Empty;
            private string _lastValue;

            public DoubleToStringConverter(string format = "G", CultureInfo culture = null)
            {
                _format = format;
                _culture = culture;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_lastValue != null) return _lastValue; // to make sure the textbox keeps the "invalid" text

                var cultureSafe = _culture ?? culture ?? Thread.CurrentThread.CurrentUICulture;
                var result = (double?)value;
                return (result.HasValue ? result.Value.ToString(_format, cultureSafe) : string.Empty) + _attachDot;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                _lastValue = null; // we assume it will work

                var cultureSafe = _culture ?? culture ?? Thread.CurrentThread.CurrentUICulture;
                var separator = cultureSafe.NumberFormat.CurrencyDecimalSeparator;

                double? result = null;
                var str = (string)value ?? string.Empty;
                _attachDot = str.EndsWith(separator) ? separator : string.Empty;

                double d;
                if (double.TryParse(str, NumberStyles.Float, cultureSafe, out d))
                    result = d;
                else
                    _lastValue = str;

                return result;
            }
        }

        #endregion

        public NZazuDoubleField(string key) : base(key) { }

        public override string Type => "double";
        public override DependencyProperty ContentProperty => TextBox.TextProperty;

        protected override Control GetValue()
        {
            var result = new TextBox { ToolTip = Description };
            return result;
        }

        protected override void SetStringValue(string value)
        {
            double result;
            if (!string.IsNullOrWhiteSpace(value) &&
                double.TryParse(value, NumberStyles.Number, FormatProvider, out result))
                Value = result;
            else
                Value = null;
        }

        protected override string GetStringValue()
        {
            return Value.HasValue ? Value.Value.ToString(FormatProvider) : string.Empty;
        }

        protected override Binding DecorateBinding(Binding binding)
        {
            var format = Settings.ContainsKey("Format") ? Settings["Format"] : "G";
            var decorated = base.DecorateBinding(binding);
            decorated.Converter = new DoubleToStringConverter(format);
            return decorated;
        }
    }
}