using MU.Global.Enums;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace MU.Global.ImageConverter
{
    public class LEDImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage result = null;

            switch ((LEDStatus) value)
            {
                case LEDStatus.OFF:
                default:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_OFF.png"));
                    break;
                case LEDStatus.RED:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_RED.png"));
                    break;
                case LEDStatus.BLUE:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_BLUE.png"));
                    break;
                case LEDStatus.GREEN:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_GREEN.png"));
                    break;
            }

            if (result == null)
            {
                result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_OFF.png"));
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
