using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using PowerManager.Core.Services;
using PowerManager.Core.Services.Implementations;
using PowerManager.UI.Services;
using PowerManager.UI.ViewModels;

namespace PowerManager.UI;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        this.InitializeComponent();
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(configure =>
        {
            configure.AddDebug();
            configure.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IWingetService, WingetService>();
        services.AddSingleton<IQueueService, QueueService>();
        services.AddSingleton<IUiDispatcher, UiDispatcher>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<CatalogViewModel>();
        services.AddTransient<QueueViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private Window? m_window;
}