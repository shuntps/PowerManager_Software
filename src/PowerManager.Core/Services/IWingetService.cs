using PowerManager.Core.Models;

namespace PowerManager.Core.Services;

public interface IWingetService
{
    Task<bool> CheckWingetInstalledAsync();
    Task<Package?> GetPackageInfoAsync(string id, CancellationToken cancellationToken = default);
    Task<Package?> GetPackageDetailsAsync(string id, CancellationToken cancellationToken = default);
    Task InstallPackageAsync(string id, CancellationToken cancellationToken = default);
    Task UninstallPackageAsync(string id, CancellationToken cancellationToken = default);
    Task UpgradePackageAsync(string id, CancellationToken cancellationToken = default);
}
