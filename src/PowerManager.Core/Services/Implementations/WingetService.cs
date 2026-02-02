using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<List<Package>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
    {
        _ = await RunWingetCommandAsync("list", cancellationToken);
        return [];
    }

    public async Task<List<Package>> SearchPackagesAsync(string query, CancellationToken cancellationToken = default)
    {
        _ = await RunWingetCommandAsync($"search \"{query}\"", cancellationToken);
        return [];
    }

    public async Task<Package?> GetPackageDetailsAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = await RunWingetCommandAsync($"show --id {id}", cancellationToken);
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

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = errorBuilder.ToString();
                LogWingetCommandFailed(logger, arguments, error);
                throw new Exception($"Winget failed with exit code {process.ExitCode}: {error}");
            }

            return outputBuilder.ToString();
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
            throw;
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
}
