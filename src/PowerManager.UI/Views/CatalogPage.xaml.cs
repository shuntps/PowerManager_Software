using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using PowerManager.UI.ViewModels;
using PowerManager.Core.Models;
using System.Linq;

namespace PowerManager.UI.Views;

public sealed partial class CatalogPage : Page
{
    public CatalogViewModel ViewModel { get; }

    public CatalogPage()
    {
        this.InitializeComponent();
        ViewModel = ((App)App.Current).Services.GetRequiredService<CatalogViewModel>();
        this.DataContext = ViewModel;
    }

    private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadCatalogCommand.ExecuteAsync(null);
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView)
        {
            ViewModel.SelectedPackages.Clear();
            foreach (var item in listView.SelectedItems.OfType<Package>())
            {
                ViewModel.SelectedPackages.Add(item);
            }
            ViewModel.SelectedCount = ViewModel.SelectedPackages.Count;
        }
    }
}
