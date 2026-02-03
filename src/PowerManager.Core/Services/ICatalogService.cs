using PowerManager.Core.Models;

namespace PowerManager.Core.Services;

public interface ICatalogService
{
    Task<List<Package>> GetMergedCatalogAsync(CancellationToken cancellationToken = default);
    Task<List<Package>> GetDefaultCatalogAsync();
    Task<List<Package>> GetCustomCatalogAsync();
    Task AddToCustomCatalogAsync(Package package);
    Task RemoveFromCustomCatalogAsync(string packageId);
    Task RefreshPackageStatusAsync(Package package, CancellationToken cancellationToken = default);
    Task SaveCatalogAsync(List<Package> packages);
}
