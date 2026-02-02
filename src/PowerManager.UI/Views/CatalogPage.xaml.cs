using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using PowerManager.UI.ViewModels;
using PowerManager.Core.Models;
using System;

namespace PowerManager.UI.Views;

public sealed partial class CatalogPage : Page
{
    public CatalogViewModel ViewModel { get; }

    public CatalogPage()
    {
        this.InitializeComponent();
        ViewModel = ((App)App.Current).Services.GetRequiredService<CatalogViewModel>();
    }

    private void InstallButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.Button button && button.Tag is Package package)
        {
            ViewModel.InstallPackageCommand.Execute(package);
        }
    }
}
