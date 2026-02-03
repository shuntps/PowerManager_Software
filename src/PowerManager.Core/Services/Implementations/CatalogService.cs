using System.Text;
using Microsoft.Extensions.Logging;
using PowerManager.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PowerManager.Core.Services.Implementations;

public partial class CatalogService(
IWingetService wingetService,
ILogger<CatalogService> logger) : ICatalogService
{
    private readonly string _dataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PowerManager Software");
    
    private readonly string _defaultCatalogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PowerManager Software",
        "catalog_default.yaml");
    
    private readonly string _customCatalogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PowerManager Software",
        "catalog_custom.yaml");

    public async Task<List<Package>> GetMergedCatalogAsync(CancellationToken cancellationToken = default)
    {
        var defaultCatalog = await GetDefaultCatalogAsync();
        LogDefaultCatalogCount(logger, defaultCatalog.Count);
        
        var customCatalog = await GetCustomCatalogAsync();
        LogCustomCatalogCount(logger, customCatalog.Count);

        var merged = new Dictionary<string, Package>();

        foreach (var pkg in defaultCatalog)
        {
            merged[pkg.Id] = pkg;
        }

        foreach (var pkg in customCatalog)
        {
            merged[pkg.Id] = pkg;
        }

        var result = merged.Values.ToList();
        LogMergedCatalogCount(logger, result.Count);
        
        // Don't call RefreshInstallationStatusAsync here
        // It will be called in background by ViewModel after initial load
        
        return result;
    }

    public async Task<List<Package>> GetDefaultCatalogAsync()
    {
        if (!File.Exists(_defaultCatalogPath))
        {
            LogCreatingDefaultCatalog(logger, _defaultCatalogPath);
            await CreateDefaultCatalogAsync();
        }

        return await LoadCatalogFromFileAsync(_defaultCatalogPath);
    }

    public async Task<List<Package>> GetCustomCatalogAsync()
    {
        if (!File.Exists(_customCatalogPath))
        {
            return [];
        }

        return await LoadCatalogFromFileAsync(_customCatalogPath);
    }

    public async Task AddToCustomCatalogAsync(Package package)
    {
        var catalog = await GetCustomCatalogAsync();
        
        if (catalog.Any(p => p.Id == package.Id))
        {
            LogPackageAlreadyInCatalog(logger, package.Id);
            return;
        }

        catalog.Add(package);
        await SaveCatalogToFileAsync(_customCatalogPath, catalog);
        LogPackageAddedToCatalog(logger, package.Id);
    }

    public async Task RemoveFromCustomCatalogAsync(string packageId)
    {
        var catalog = await GetCustomCatalogAsync();
        var removed = catalog.RemoveAll(p => p.Id == packageId);

        if (removed > 0)
        {
            await SaveCatalogToFileAsync(_customCatalogPath, catalog);
            LogPackageRemovedFromCatalog(logger, packageId);
        }
    }

    public async Task RefreshPackageStatusAsync(Package package, CancellationToken cancellationToken = default)
    {
        try
        {
            var packageInfo = await wingetService.GetPackageInfoAsync(package.Id, cancellationToken);
            
            if (packageInfo != null)
            {
                package.Source = packageInfo.Source;
                package.IsInstalled = packageInfo.IsInstalled;
                package.InstalledVersion = packageInfo.InstalledVersion;
                package.AvailableVersion = packageInfo.AvailableVersion;
                package.UpdateAvailable = packageInfo.UpdateAvailable;
                package.LastChecked = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            LogPackageRefreshFailed(logger, package.Id, ex);
        }
    }

    public async Task SaveCatalogAsync(List<Package> packages)
    {
        try
        {
            await SaveCatalogToFileAsync(_defaultCatalogPath, packages);
            LogCatalogSaved(logger, packages.Count);
        }
        catch (Exception ex)
        {
            LogCatalogSaveFailed(logger, _defaultCatalogPath, ex);
        }
    }

    private async Task CreateDefaultCatalogAsync()
    {
        Directory.CreateDirectory(_dataFolder);

        var defaultPackages = new List<Package>
        {
            new()
            {
                Id = "Google.Chrome",
                Name = "Google Chrome",
                Category = "Browsers",
                Tags = ["browser", "popular", "google"],
                Description = "Fast and secure web browser"
            },
            new()
            {
                Id = "7zip.7zip",
                Name = "7-Zip",
                Category = "Utilities",
                Tags = ["compression", "archive", "utility"],
                Description = "File archiver with high compression ratio"
            },
            new()
            {
                Id = "Microsoft.VisualStudioCode",
                Name = "Visual Studio Code",
                Category = "Development",
                Tags = ["editor", "coding", "popular", "microsoft"],
                Description = "Code editor with support for debugging and extensions"
            },
            new()
            {
                Id = "Discord.Discord",
                Name = "Discord",
                Category = "Communication",
                Tags = ["chat", "voice", "gaming", "popular"],
                Description = "Voice, video, and text communication platform"
            },
            new()
            {
                Id = "Notepad++.Notepad++",
                Name = "Notepad++",
                Category = "Development",
                Tags = ["editor", "text", "coding"],
                Description = "Free source code editor and Notepad replacement"
            }
        };

        await SaveCatalogToFileAsync(_defaultCatalogPath, defaultPackages);
    }

    private async Task<List<Package>> LoadCatalogFromFileAsync(string path)
    {
        try
        {
            var yaml = await File.ReadAllTextAsync(path, Encoding.UTF8);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize<CatalogConfig>(yaml);
            return config.Packages;
        }
        catch (Exception ex)
        {
            LogCatalogLoadFailed(logger, path, ex);
            return [];
        }
    }

    private async Task SaveCatalogToFileAsync(string path, List<Package> packages)
    {
        try
        {
            Directory.CreateDirectory(_dataFolder);

            var config = new CatalogConfig { Packages = packages };
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(config);
            await File.WriteAllTextAsync(path, yaml, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            LogCatalogSaveFailed(logger, path, ex);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Creating default catalog at {Path}")]
    private static partial void LogCreatingDefaultCatalog(ILogger logger, string path);

    [LoggerMessage(LogLevel.Information, "Default catalog count: {Count}")]
    private static partial void LogDefaultCatalogCount(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "Custom catalog count: {Count}")]
    private static partial void LogCustomCatalogCount(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "Merged catalog count: {Count}")]
    private static partial void LogMergedCatalogCount(ILogger logger, int count);

    [LoggerMessage(LogLevel.Warning, "Package {PackageId} already exists in catalog")]
    private static partial void LogPackageAlreadyInCatalog(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Package {PackageId} added to catalog")]
    private static partial void LogPackageAddedToCatalog(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Package {PackageId} removed from catalog")]
    private static partial void LogPackageRemovedFromCatalog(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Error, "Failed to load catalog from {Path}")]
    private static partial void LogCatalogLoadFailed(ILogger logger, string path, Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to save catalog to {Path}")]
    private static partial void LogCatalogSaveFailed(ILogger logger, string path, Exception exception);

    [LoggerMessage(LogLevel.Warning, "Failed to check installation status")]
    private static partial void LogInstallationStatusCheckFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "WinGet refresh failed")]
    private static partial void LogWinGetRefreshFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to refresh status")]
    private static partial void LogRefreshStatusFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to save catalog")]
    private static partial void LogSaveCatalogFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Information, "Using cached versions for {Count} packages")]
    private static partial void LogUsingCachedVersions(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "Refreshing {RefreshCount} of {TotalCount} packages")]
    private static partial void LogRefreshingPackages(ILogger logger, int refreshCount, int totalCount);

    [LoggerMessage(LogLevel.Error, "Failed to refresh package {PackageId}")]
    private static partial void LogPackageRefreshFailed(ILogger logger, string packageId, Exception exception);

    [LoggerMessage(LogLevel.Information, "Catalog saved with {Count} packages")]
    private static partial void LogCatalogSaved(ILogger logger, int count);
}
