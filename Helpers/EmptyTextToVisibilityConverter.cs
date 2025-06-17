using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFModernVerticalMenu.Helpers
{
    public class EmptyTextToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(value as string);
            return (Invert ? !isEmpty : isEmpty) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
