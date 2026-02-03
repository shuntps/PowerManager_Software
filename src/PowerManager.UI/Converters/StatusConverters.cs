using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using PowerManager.Core.Models;

namespace PowerManager.UI.Converters;

public class UpdateAvailableToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool updateAvailable && updateAvailable)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InstalledNoUpdateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Package package)
        {
            return package.IsInstalled && !package.UpdateAvailable ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

