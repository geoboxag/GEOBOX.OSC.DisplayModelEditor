using System.Windows.Data;

namespace GEOBOX.OSC.DisplayModelEditor.Converters
{
    internal class EmptyStringToHyphen : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (string.IsNullOrEmpty((string)value))
                {
                    return "-";
                }
                else
                {
                    return (string)value;
                }
            }
            catch
            {
                return "-";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // No ConvertBack necessary
            return false;
        }
    }
}
