using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PowerManager.Core.Models;

namespace PowerManager.Core.Services.Implementations;

public partial class WingetService(ILogger<WingetService> logger) : IWingetService
{
    public async Task<bool> CheckWingetInstalledAsync()
    {
        try
        {
            var result = await RunWingetCommandAsync("--version", CancellationToken.None);
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }

    public async Task<Package?> GetPackageInfoAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogCheckingPackage(logger, id);
            
            // Try with --exact first for precise matching
            var listOutput = await RunWingetCommandAsync($"list --id {id} --exact --accept-source-agreements", cancellationToken);
            
            // If exact match fails, try partial match as fallback (handles cases like Google.Chrome vs Google.Chrome.EXE)
            if (string.IsNullOrWhiteSpace(listOutput) || listOutput.Contains("No installed"))
            {
                logger.LogInformation("Exact match failed for {PackageId}, trying partial match", id);
                listOutput = await RunWingetCommandAsync($"list {id} --accept-source-agreements", cancellationToken);
            }
            
            if (string.IsNullOrWhiteSpace(listOutput) || listOutput.Contains("No installed"))
            {
                LogPackageNotInstalled(logger, id);
                return new Package 
                { 
                    Id = id,
                    Source = "winget",
                    IsInstalled = false,
                    InstalledVersion = string.Empty,
                    AvailableVersion = string.Empty,
                    UpdateAvailable = false
                };
            }

            var installedVersion = ParseInstalledVersion(listOutput);
            var source = ParseSource(listOutput);
            
            // Check for updates (only if command succeeded)
            var upgradeOutput = await RunWingetCommandAsync($"upgrade --id {id} --accept-source-agreements", cancellationToken);
            var hasUpdate = false;
            var availableVersion = installedVersion;
            
            if (!string.IsNullOrWhiteSpace(upgradeOutput))
            {
                // Command succeeded - check if update is available
                hasUpdate = !upgradeOutput.Contains("No applicable") && 
                           !upgradeOutput.Contains("No installed") &&
                           !upgradeOutput.Contains("No package found");
                
                if (hasUpdate)
                {
                    availableVersion = ParseAvailableVersion(upgradeOutput);
                    // If parsing failed, no update available
                    if (string.IsNullOrWhiteSpace(availableVersion))
                    {
                        hasUpdate = false;
                        availableVersion = installedVersion;
                    }
                }
            }

            LogPackageScanned(logger, id, installedVersion, availableVersion);

            return new Package
            {
                Id = id,
                Source = source,
                IsInstalled = true,
                InstalledVersion = installedVersion,
                AvailableVersion = availableVersion,
                UpdateAvailable = hasUpdate
            };
        }
        catch (Exception ex)
        {
            LogPackageScanFailed(logger, id, ex);
            return null;
        }
    }

    private string ParseInstalledVersion(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return string.Empty;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            // Skip headers, separators, and empty lines
            if (string.IsNullOrWhiteSpace(line) ||
                line.Contains("Nom") || 
                line.Contains("Name") || 
                line.Contains("Id") ||
                line.Contains("Version") ||
                line.StartsWith("-") ||
                line.Contains("?") ||
                line.Contains("?"))
                continue;

            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Find the ID (contains dot AND has letters - not just a version number)
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (part.Contains('.') && part.Any(char.IsLetter) && i + 1 < parts.Length)
                {
                    var version = parts[i + 1];
                    // Make sure it's not a header or source
                    if (!version.Equals("Version", StringComparison.OrdinalIgnoreCase) &&
                        !version.Equals("winget", StringComparison.OrdinalIgnoreCase) &&
                        !version.Equals("msstore", StringComparison.OrdinalIgnoreCase))
                        return version;
                }
            }
        }
        return string.Empty;
    }

    private string ParseSource(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return "winget";

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) ||
                line.Contains("Nom") || 
                line.Contains("Name") || 
                line.Contains("Version") ||
                line.Contains("Source") ||
                line.StartsWith("-") ||
                line.Contains("?"))
                continue;

            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < parts.Length - 2; i++)
            {
                var part = parts[i];
                if (part.Contains('.') && part.Any(char.IsLetter) && i + 2 < parts.Length)
                {
                    var source = parts[i + 2];
                    if (source.Equals("winget", StringComparison.OrdinalIgnoreCase) ||
                        source.Equals("msstore", StringComparison.OrdinalIgnoreCase))
                        return source.ToLowerInvariant();
                }
            }
        }
        return "winget";
    }

    private string ParseAvailableVersion(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return string.Empty;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            // Skip headers, separators, and empty lines
            if (string.IsNullOrWhiteSpace(line) ||
                line.Contains("Nom") ||
                line.Contains("Name") || 
                line.Contains("Id") ||
                line.Contains("Version") ||
                line.Contains("Available") ||
                line.Contains("Disponible") ||
                line.StartsWith("-") ||
                line.Contains("?") ||
                line.Contains("?"))
                continue;

            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Find the real ID (contains dot AND has alphabetic chars)
            for (int i = 0; i < parts.Length - 2; i++)
            {
                var part = parts[i];
                if (part.Contains('.'))
                {
                    var dotParts = part.Split('.');
                    if (dotParts.Length >= 2 && 
                        dotParts[0].Any(char.IsLetter) && 
                        dotParts[1].Any(char.IsLetter) &&
                        i + 2 < parts.Length)
                    {
                        // parts[i] = ID, parts[i+1] = installed, parts[i+2] = available
                        var version = parts[i + 2];
                        if (!version.Equals("Available", StringComparison.OrdinalIgnoreCase) &&
                            !version.Equals("Disponible", StringComparison.OrdinalIgnoreCase) &&
                            !version.Equals("winget", StringComparison.OrdinalIgnoreCase) &&
                            !version.Equals("msstore", StringComparison.OrdinalIgnoreCase))
                            return version;
                    }
                }
            }
        }
        return string.Empty;
    }

    public async Task<Package?> GetPackageDetailsAsync(string id, CancellationToken cancellationToken = default)
    {
        var output = await RunWingetCommandAsync($"show --id {id} --exact --accept-source-agreements", cancellationToken);
        
        if (string.IsNullOrWhiteSpace(output))
            return null;

        return new Package { Id = id };
    }

    public async Task InstallPackageAsync(string id, CancellationToken cancellationToken = default)
    {
        LogInstallingPackage(logger, id);
        await RunWingetCommandAsync($"install --id {id} --silent --accept-package-agreements --accept-source-agreements", cancellationToken);
    }

    public async Task UninstallPackageAsync(string id, CancellationToken cancellationToken = default)
    {
        LogUninstallingPackage(logger, id);
        await RunWingetCommandAsync($"uninstall --id {id} --silent", cancellationToken);
    }

    public async Task UpgradePackageAsync(string id, CancellationToken cancellationToken = default)
    {
        LogUpgradingPackage(logger, id);
        await RunWingetCommandAsync($"upgrade --id {id} --silent --accept-package-agreements --accept-source-agreements", cancellationToken);
    }

    private async Task<string> RunWingetCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "winget",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = processInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Timeout de 10 secondes par package
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);

            if (process.ExitCode != 0)
            {
                var error = errorBuilder.ToString();
                LogWingetCommandFailed(logger, arguments, error);
                return string.Empty; // Don't throw - just return empty
            }

            return outputBuilder.ToString();
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
            LogWingetCommandTimeout(logger, arguments);
            return string.Empty; // Don't throw - just return empty
        }
        catch (Exception ex)
        {
            LogWingetCommandException(logger, arguments, ex);
            return string.Empty; // Don't throw - just return empty
        }
    }

    [LoggerMessage(LogLevel.Information, "Installing package {PackageId}")]
    private static partial void LogInstallingPackage(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Uninstalling package {PackageId}")]
    private static partial void LogUninstallingPackage(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Upgrading package {PackageId}")]
    private static partial void LogUpgradingPackage(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Error, "Winget command failed: {Arguments}. Error: {Error}")]
    private static partial void LogWingetCommandFailed(ILogger logger, string arguments, string error);

    [LoggerMessage(LogLevel.Warning, "Winget command timeout: {Arguments}")]
    private static partial void LogWingetCommandTimeout(ILogger logger, string arguments);

    [LoggerMessage(LogLevel.Error, "Winget command exception: {Arguments}")]
    private static partial void LogWingetCommandException(ILogger logger, string arguments, Exception exception);

    [LoggerMessage(LogLevel.Information, "Checking package {PackageId}")]
    private static partial void LogCheckingPackage(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Debug, "Package {PackageId} not installed")]
    private static partial void LogPackageNotInstalled(ILogger logger, string packageId);

    [LoggerMessage(LogLevel.Information, "Package {PackageId} scanned - Installed: {InstalledVersion}, Available: {AvailableVersion}")]
    private static partial void LogPackageScanned(ILogger logger, string packageId, string installedVersion, string availableVersion);

    [LoggerMessage(LogLevel.Error, "Failed to scan package {PackageId}")]
    private static partial void LogPackageScanFailed(ILogger logger, string packageId, Exception exception);

    [LoggerMessage(LogLevel.Debug, "Raw WinGet list output for {PackageId}: {Output}")]
    private static partial void LogRawListOutput(ILogger logger, string packageId, string output);
}
