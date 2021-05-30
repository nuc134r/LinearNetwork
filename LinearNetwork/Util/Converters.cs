using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LinearNetwork.Util
{
    public class BooleanConverter<T> : MarkupExtension, IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public sealed class BoolToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BoolToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }

    public sealed class InvertedBoolToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InvertedBoolToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible) { }
    }

    public sealed class InvertedBoolConverter : BooleanConverter<bool>
    {
        public InvertedBoolConverter() : base(false, true) { }
    }
}
