using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PowerManager.Core.Models;
using PowerManager.Core.Services;

namespace PowerManager.UI.ViewModels;

public partial class CatalogViewModel(
    ICatalogService catalogService,
    IQueueService queueService,
    IUiDispatcher dispatcher,
    ILogger<CatalogViewModel> logger) : ObservableObject
{
    private List<Package> _allPackages = [];

    [ObservableProperty]
    private ObservableCollection<Package> _packages = [];

    [ObservableProperty]
    private ObservableCollection<Package> _selectedPackages = [];

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _scanStatus = string.Empty;

    [ObservableProperty]
    private double _scanProgress;

    [ObservableProperty]
    private string _scanProgressText = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All Categories";

    [ObservableProperty]
    private ObservableCollection<string> _categories = ["All Categories"];

    [RelayCommand]
    private async Task LoadCatalogAsync()
    {
        IsLoading = true;
        
        // Subscribe to completion events on first load
        queueService.ItemCompleted -= OnQueueItemCompleted; // Unsubscribe first to avoid duplicates
        queueService.ItemCompleted += OnQueueItemCompleted;
        
        try 
        {
            LogLoadingCatalog(logger);
            
            // Load catalog structure (fast)
            var catalog = await catalogService.GetMergedCatalogAsync();
            _allPackages = catalog.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
            
            var uniqueCategories = _allPackages
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            Categories.Clear();
            Categories.Add("All Categories");
            foreach (var cat in uniqueCategories)
            {
                Categories.Add(cat);
            }

            ApplyFilters();
            
            // Now scan packages with progress
            await ScanPackagesAsync();
            
            LogCatalogLoaded(logger, catalog.Count);
        }
        catch (Exception ex)
        {
            LogLoadCatalogFailed(logger, ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ScanPackagesAsync()
    {
        IsScanning = true;
        var total = _allPackages.Count;
        var scanned = 0;

        try
        {
            foreach (var package in _allPackages)
            {
                try
                {
                    ScanStatus = $"Checking {package.Name}...";
                    
                    await catalogService.RefreshPackageStatusAsync(package);
                    
                    scanned++;
                    ScanProgress = (scanned * 100.0) / total;
                    ScanProgressText = $"{scanned}/{total} packages scanned";
                    
                    // Small delay for visual feedback
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    // Log but continue with next package
                    logger.LogError(ex, "Failed to scan package {PackageId}", package.Id);
                }
            }

            ScanStatus = "Saving catalog...";
            await catalogService.SaveCatalogAsync(_allPackages);
            
            ScanStatus = "Complete!";
            await Task.Delay(300);
            
            logger.LogInformation("Scan complete, refreshing display...");
            
            // Refresh display with updated data - MUST be on UI thread
            dispatcher.TryEnqueue(() => ApplyFilters());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during scan");
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async void OnQueueItemCompleted(object? sender, QueueItem item)
    {
        logger.LogInformation("Queue item completed: {Action} on {PackageId}, refreshing status", item.Action, item.PackageId);
        
        // Find the package in the catalog
        var package = _allPackages.FirstOrDefault(p => p.Id == item.PackageId);
        if (package == null)
        {
            logger.LogWarning("Package {PackageId} not found in catalog after operation", item.PackageId);
            return;
        }

        // Refresh the package status asynchronously
        try
        {
            await catalogService.RefreshPackageStatusAsync(package);
            await catalogService.SaveCatalogAsync(_allPackages);
            
            // Update UI on UI thread
            dispatcher.TryEnqueue(() =>
            {
                ApplyFilters(); // Refresh the displayed list
                logger.LogInformation("Package {PackageId} status refreshed after {Action}", item.PackageId, item.Action);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh package {PackageId} after operation", item.PackageId);
        }
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var filtered = _allPackages.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(p => 
                p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Id.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedCategory != "All Categories" && !string.IsNullOrEmpty(SelectedCategory))
        {
            filtered = filtered.Where(p => p.Category == SelectedCategory);
        }

        Packages.Clear();
        foreach (var pkg in filtered)
        {
            Packages.Add(pkg);
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void InstallSelected()
    {
        if (SelectedPackages.Count == 0)
        {
            LogNoPackagesSelected(logger);
            return;
        }

        foreach (var package in SelectedPackages.Where(p => !p.IsInstalled))
        {
            LogEnqueueingPackage(logger, package.Id);
            queueService.Enqueue(new QueueItem 
            { 
                PackageId = package.Id, 
                Action = "Install",
                Status = Core.Enums.QueueItemStatus.Pending
            });
        }

        SelectedPackages.Clear();
        SelectedCount = 0;
    }

    [RelayCommand]
    private void UninstallSelected()
    {
        if (SelectedPackages.Count == 0)
        {
            LogNoPackagesSelected(logger);
            return;
        }

        foreach (var package in SelectedPackages.Where(p => p.IsInstalled))
        {
            LogEnqueueingUninstall(logger, package.Id);
            queueService.Enqueue(new QueueItem
            {
                PackageId = package.Id,
                Action = "Uninstall",
                Status = Core.Enums.QueueItemStatus.Pending
            });
        }

        SelectedPackages.Clear();
        SelectedCount = 0;
    }

    [RelayCommand]
    private void UpdateSelected()
    {
        if (SelectedPackages.Count == 0)
        {
            LogNoPackagesSelected(logger);
            return;
        }

        foreach (var package in SelectedPackages.Where(p => p.IsInstalled))
        {
            LogEnqueueingUpdate(logger, package.Id);
            queueService.Enqueue(new QueueItem
            {
                PackageId = package.Id,
                Action = "Upgrade",
                Status = Core.Enums.QueueItemStatus.Pending
            });
        }

        SelectedPackages.Clear();
        SelectedCount = 0;
    }

    [LoggerMessage(LogLevel.Information, "Loading catalog")]
    private static partial void LogLoadingCatalog(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Received {Count} packages from catalog service")]
    private static partial void LogCatalogReceived(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "All packages count: {Count}")]
    private static partial void LogAllPackagesCount(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "Categories count: {Count}")]
    private static partial void LogCategoriesCount(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "Packages displayed after filters: {Count}")]
    private static partial void LogPackagesDisplayed(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "Loaded catalog with {Count} packages")]
    private static partial void LogCatalogLoaded(ILogger logger, int count);

    [LoggerMessage(LogLevel.Error, "Failed to load catalog")]
    private static partial void LogLoadCatalogFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Warning, "No packages selected")]
    private static partial void LogNoPackagesSelected(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Enqueueing package {PackageId} for installation")]
    private static partial void LogEnqueueingPackage(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Enqueueing package {PackageId} for uninstall")]
    private static partial void LogEnqueueingUninstall(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Enqueueing package {PackageId} for update")]
    private static partial void LogEnqueueingUpdate(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Error, "Background refresh failed")]
    private static partial void LogBackgroundRefreshFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Information, "Background refresh completed")]
    private static partial void LogBackgroundRefreshCompleted(ILogger logger);
}
