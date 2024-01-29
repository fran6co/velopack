﻿using Microsoft.Extensions.Logging;
using Velopack.Packaging;
using Velopack.Packaging.Abstractions;
using Velopack.Sources;

namespace Velopack.Deployment;

public class RepositoryOptions : IOutputOptions
{
    public string Channel { get; set; } = ReleaseEntryHelper.GetDefaultChannel();

    public DirectoryInfo ReleaseDir { get; set; }
}

public interface IRepositoryCanUpload<TUp> where TUp : RepositoryOptions
{
    Task UploadMissingAssetsAsync(TUp options);
}

public interface IRepositoryCanDownload<TDown> where TDown : RepositoryOptions
{
    Task DownloadLatestFullPackageAsync(TDown options);
}

public abstract class SourceRepository<TDown, TSource> : DownRepository<TDown>
    where TDown : RepositoryOptions
    where TSource : IUpdateSource
{
    public SourceRepository(ILogger logger)
        : base(logger)
    { }

    protected override Task<VelopackAssetFeed> GetReleasesAsync(TDown options)
    {
        var source = CreateSource(options);
        return source.GetReleaseFeed(channel: options.Channel, logger: Log);
    }

    protected override Task SaveEntryToFileAsync(TDown options, VelopackAsset entry, string filePath)
    {
        var source = CreateSource(options);
        return source.DownloadReleaseEntry(Log, entry, filePath, (i) => { });
    }

    public abstract TSource CreateSource(TDown options);
}

public abstract class DownRepository<TDown> : IRepositoryCanDownload<TDown>
    where TDown : RepositoryOptions
{
    protected ILogger Log { get; }

    public DownRepository(ILogger logger)
    {
        Log = logger;
    }

    public virtual async Task DownloadLatestFullPackageAsync(TDown options)
    {
        VelopackAssetFeed feed = await RetryAsyncRet(() => GetReleasesAsync(options), $"Fetching releases for channel {options.Channel}...");
        var releases = feed.Assets;

        Log.Info($"Found {releases.Length} release in remote file");

        var latest = releases.Where(r => r.Type == VelopackAssetType.Full).OrderByDescending(r => r.Version).FirstOrDefault();
        if (latest == null) {
            Log.Warn("No full / applicable release was found to download. Aborting.");
            return;
        }

        var path = Path.Combine(options.ReleaseDir.FullName, latest.FileName);
        var incomplete = Path.Combine(options.ReleaseDir.FullName, latest.FileName + ".incomplete");

        if (File.Exists(path)) {
            Log.Warn($"File '{path}' already exists on disk. Verifying checksum...");
            var hash = Utility.CalculateFileSHA1(path);
            if (hash == latest.SHA1) {
                Log.Info("Checksum matches. Finished.");
                return;
            } else {
                Log.Info($"Checksum mismatch, re-downloading...");
            }
        }

        await RetryAsync(() => SaveEntryToFileAsync(options, latest, incomplete), $"Downloading {latest.FileName}...");

        Log.Info("Verifying checksum...");
        var newHash = Utility.CalculateFileSHA1(incomplete);
        if (newHash != latest.SHA1) {
            Log.Error($"Checksum mismatch, expected {latest.SHA1}, got {newHash}");
            return;
        }

        File.Move(incomplete, path, true);
        Log.Info("Finished.");
    }

    protected abstract Task<VelopackAssetFeed> GetReleasesAsync(TDown options);

    protected abstract Task SaveEntryToFileAsync(TDown options, VelopackAsset entry, string filePath);

    protected async Task<T> RetryAsyncRet<T>(Func<Task<T>> block, string message, int maxRetries = 1)
    {
        int ctry = 0;
        while (true) {
            try {
                Log.Info((ctry > 0 ? $"(retry {ctry}) " : "") + message);
                return await block().ConfigureAwait(false);
            } catch (Exception ex) {
                if (ctry++ > maxRetries) {
                    Log.Error(ex.Message + ", will not try again.");
                    throw;
                }

                Log.Error($"{ex.Message}, retrying in 1 second.");
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }

    protected async Task RetryAsync(Func<Task> block, string message, int maxRetries = 1)
    {
        int ctry = 0;
        while (true) {
            try {
                Log.Info((ctry > 0 ? $"(retry {ctry}) " : "") + message);
                await block().ConfigureAwait(false);
                return;
            } catch (Exception ex) {
                if (ctry++ > maxRetries) {
                    Log.Error(ex.Message + ", will not try again.");
                    throw;
                }

                Log.Error($"{ex.Message}, retrying in 1 second.");
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
