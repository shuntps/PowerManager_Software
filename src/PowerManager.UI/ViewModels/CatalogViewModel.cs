using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PowerManager.Core.Models;
using PowerManager.Core.Services;

namespace PowerManager.UI.ViewModels;

public partial class CatalogViewModel(
    IWingetService wingetService,
    IQueueService queueService,
    ILogger<CatalogViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Package> _packages = [];

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadPackagesAsync()
    {
        IsLoading = true;
        try 
        {
            LogLoadingPackages(logger);
            var packages = await wingetService.GetInstalledPackagesAsync();
            Packages.Clear();
            foreach (var pkg in packages)
            {
                Packages.Add(pkg);
            }
            LogPackagesLoaded(logger, packages.Count);
        }
        catch (Exception ex)
        {
            LogLoadPackagesFailed(logger, ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void InstallPackage(Package package)
    {
        LogEnqueueingPackage(logger, package.Id);
        queueService.Enqueue(new QueueItem 
        { 
            PackageId = package.Id, 
            Action = "Install",
            Status = Core.Enums.QueueItemStatus.Pending
        });
    }

    [LoggerMessage(LogLevel.Information, "Loading installed packages")]
    private static partial void LogLoadingPackages(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Loaded {Count} packages")]
    private static partial void LogPackagesLoaded(ILogger logger, int count);

    [LoggerMessage(LogLevel.Error, "Failed to load packages")]
    private static partial void LogLoadPackagesFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Information, "Enqueueing package {PackageId} for installation")]
    private static partial void LogEnqueueingPackage(ILogger logger, string packageId);
}
