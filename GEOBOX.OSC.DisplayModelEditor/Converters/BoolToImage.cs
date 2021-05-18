using System.Windows.Data;

namespace GEOBOX.OSC.DisplayModelEditor.Converters
{
    internal class BoolToImage : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object paramter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                {
                    return "/Includes/gbLogSuccess16.png";
                }
                else
                {
                    return "/Includes/gbLogError16.png";
                }
            }
            catch
            {
                return "/Includes/gbLogError16.png";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // No ConvertBack necessary
            return false;
        }
    }
}
