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
        
        // Handle unhandled exceptions to prevent app crash
        this.UnhandledException += OnUnhandledException;
        
        Services = ConfigureServices();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Log the exception with full details
        var logger = Services.GetService<ILogger<App>>();
        logger?.LogError(e.Exception, "UNHANDLED EXCEPTION: {Message}", e.Exception.Message);
        logger?.LogError("Stack trace: {StackTrace}", e.Exception.StackTrace);
        
        // DON'T mark as handled - let it crash so we can see the real error!
        // e.Handled = true;
        
        logger?.LogInformation("Unhandled exception logged, app will crash to show debugger");
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
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<IUiDispatcher, UiDispatcher>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<CatalogViewModel>();
        services.AddTransient<QueueViewModel>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = Services.GetRequiredService<MainWindow>();
        m_window.Activate();
    }

    private Window? m_window;
}