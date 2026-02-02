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
		this.InitializeComponent();
        
        var dispatcher = ((App)App.Current).Services.GetRequiredService<IUiDispatcher>();
        if (dispatcher is UiDispatcher uiDispatcher)
        {
            uiDispatcher.Initialize(this.DispatcherQueue);
        }
        
        NavView.Loaded += (s, e) => 
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        };
	}

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            Type? pageType = null;
            if (item.Tag?.ToString() == "CatalogPage") pageType = typeof(CatalogPage);
            else if (item.Tag?.ToString() == "QueuePage") pageType = typeof(QueuePage);

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}