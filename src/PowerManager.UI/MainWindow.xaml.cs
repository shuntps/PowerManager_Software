using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerManager.UI.Views;
using PowerManager.UI.Services;
using PowerManager.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace PowerManager.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (((App)App.Current).Services.GetRequiredService<IUiDispatcher>() is UiDispatcher dispatcher)
        {
            dispatcher.Initialize(DispatcherQueue);
        }

        NavView.Loaded += (_, _) => NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item)
            return;

        var pageType = item.Tag?.ToString() switch
        {
            "CatalogPage" => typeof(CatalogPage),
            "QueuePage" => typeof(QueuePage),
            _ => null
        };

        if (pageType is not null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
