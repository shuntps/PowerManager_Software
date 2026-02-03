using PowerManager.Core.Models;

namespace PowerManager.Core.Services;

public interface IWingetService
{
    Task<bool> CheckWingetInstalledAsync();
    Task<List<Package>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default);
    Task<List<Package>> SearchPackagesAsync(string query, CancellationToken cancellationToken = default);
    Task<Package?> GetPackageDetailsAsync(string id, CancellationToken cancellationToken = default);
    Task InstallPackageAsync(string id, CancellationToken cancellationToken = default); // Should probably stream logs or progress?
    Task UninstallPackageAsync(string id, CancellationToken cancellationToken = default);
    Task UpgradePackageAsync(string id, CancellationToken cancellationToken = default);
}
