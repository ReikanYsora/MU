using System;
using Windows.Devices.WiFi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace MU.Global.ImageConverter
{
    public class LEDWiFiImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage result = null;

            switch ((WiFiConnectionStatus) value)
            {
                case WiFiConnectionStatus.Success:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_BLUE.png"));
                    break;
                case WiFiConnectionStatus.InvalidCredential:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_RED.png"));
                    break;
                case WiFiConnectionStatus.AccessRevoked:
                case WiFiConnectionStatus.NetworkNotAvailable:
                case WiFiConnectionStatus.Timeout:
                case WiFiConnectionStatus.UnspecifiedFailure:
                case WiFiConnectionStatus.UnsupportedAuthenticationProtocol:
                default:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/LED/LED_OFF.png"));
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
