using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using PowerManager.UI.ViewModels;
using System;

namespace PowerManager.UI.Views;

public sealed partial class QueuePage : Page
{
    public QueueViewModel ViewModel { get; }

    public QueuePage()
    {
        this.InitializeComponent();
        ViewModel = ((App)App.Current).Services.GetRequiredService<QueueViewModel>();
    }
}
