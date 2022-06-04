using MU.Managers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace MU.Global.ImageConverter
{
    public class ExpressionImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage result = null;

            switch ((Expression) value)
            {
                case Expression.EyesClosed:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Eye_Closed.png"));
                    break;
                case Expression.Happy:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Happy.png"));
                    break;
                case Expression.Angry:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Angry.png"));
                    break;
                case Expression.Sad:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Sad.png"));
                    break;
                case Expression.Surprised:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Surprised.png"));
                    break;
                case Expression.Jaded:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Jaded.png"));
                    break;
                case Expression.Skeptic:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Skeptic.png"));
                    break;
                case Expression.Normal:
                default:
                    result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Normal.png"));
                    break;
            }

            if (result == null)
            {
                result = new BitmapImage(new Uri("ms-appx://MU/Resources/Images/Expressions/MU_Exp_Eye_Closed.png"));
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
