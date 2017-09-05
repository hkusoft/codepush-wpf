using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace codepush_wpf
{
    public class OsToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string statusValue = parameter.ToString().ToUpper();

            if (!string.IsNullOrEmpty(statusValue))
            {
                string result = string.Empty;

                switch (statusValue)
                {
                    case "Android":
                        result = "Android.png";
                        break;
                    case "iOS":
                        result = "iOS.png";
                        break;
                    default:
                        result = "Android.png";
                        break;
                }

                var uri = new Uri("pack://application:,,,/Icons/" + result);

                return uri;
            }

            return string.Empty;
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
