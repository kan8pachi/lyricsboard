using LyricsBoard.Core.BeatSaber;
using LyricsBoard.Core.System;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    /// <summary>
    /// The public interface for SondDefinition users.
    /// </summary>
    internal class SongDefinitionManager
    {
        private class CancellableTask<T> : IDisposable
        {
            public Task<T> TTask { get; private set; }
            public CancellationTokenSource TokenSource { get; private set; }
            private CancellableTask(Task<T> task, CancellationTokenSource cancellationTokenSource)
            {
                TTask = task;
                TokenSource = cancellationTokenSource;
            }

            public static CancellableTask<T> Run(Func<T> action)
            {
                var tokenSource = new CancellationTokenSource();
                var task = Task.Run(action, tokenSource.Token);
                return new CancellableTask<T>(task, tokenSource);
            }

            public async Task CancelSafelyAsync()
            {
                if (TTask.IsCompleted)
                {
                    TokenSource.Cancel();
                    try
                    {
                        await TTask;
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        TokenSource.Dispose();
                    }
                }
            }

            public void Dispose()
            {
                ((IDisposable)TTask).Dispose();
            }
        }

        private K8Logger? logger;
        private LRUMemoryCache<string, Task<SongDefinition>> songCache;
        private readonly ISongDefinitionLoader sdLoader;
        private CancellableTask<ISongCatalog>? songCatalogTask;

        public SongDefinitionManager(K8Logger? logger, ISongDefinitionLoader sdLoader, int cacheCapacity)
        {
            this.logger = logger;
            songCache = new LRUMemoryCache<string, Task<SongDefinition>>(cacheCapacity);
            this.sdLoader = sdLoader;
        }

        /// <summary>
        /// Return SongDefinition from cache. If not found, load from file and cache it, and return it.
        /// </summary>
        /// <param name="songHash">Song hash. Must not be null.</param>
        /// <returns>Never be null. If not found, Lyrics property will be null.</returns>
        public async Task<SongDefinition> GetSongDefinition(string songHash)
        {
            if (songCatalogTask == null)
            {
                var msg = $"tried to load song definition for hash [{songHash}] but manager not initialized.";
                msg += " skipped loading song definition.";
                msg += " if you see this message repeatedly, it generally means a bug of the program.";
                logger?.Error(msg);
                return new SongDefinition(songHash);
            }

            var sdTask = songCache.GetOrCreate(
                songHash,
                async () =>
                {
                    var songCatalog = await songCatalogTask.TTask;
                    return await Task.Run(() => songCatalog.LoadByHash(songHash));
                }
            );
            var sd = await sdTask;
            logger?.Info($"finished loading song definition for hash [{songHash}].");
            return sd;
        }

        public async Task InitializeAsync()
        {
            await ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            logger?.Debug("start loading catalog and reseting cache ...");

            songCache.Clear();

            // replace with new instance for thread safety.
            songCache = new LRUMemoryCache<string, Task<SongDefinition>>(songCache.Capacity);

            var prevTask = songCatalogTask;
            if (prevTask != null)
            {
                logger?.Info("detected previous catalog loading task. cancelling it ...");
                await prevTask.CancelSafelyAsync();
                prevTask.Dispose();
                songCatalogTask = null;
                logger?.Info("finished cancelling previous catalog loading task.");
            }
            var newTask = CancellableTask<ISongCatalog>.Run(() => sdLoader.BuildSongCatalog());

            songCatalogTask = newTask;
            await newTask.TTask;

            logger?.Info("finished loading catalog.");
        }
    }
}
