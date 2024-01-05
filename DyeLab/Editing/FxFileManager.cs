using DyeLab.Configuration;

namespace DyeLab.Editing;

public class FxFileManager : IDisposable
{
    private readonly Config _config;
    private readonly FileSystemWatcher _watcher = new();
    private FileInfo? _openFile;

    public event Action<string>? Changed;

    private bool _isDisposed;

    public FxFileManager(Config config)
    {
        _config = config;

        _watcher.Changed += OnUpdate;
        _watcher.Renamed += OnFileDeleted;
        _watcher.Deleted += OnFileDeleted;
    }

    public void OpenFxFile()
    {
        var path = _config.FxFilePath.Value;
        var fileInfo = new FileInfo(path);

        if (!fileInfo.Exists)
        {
            _config.FxFilePath.Clear();
            fileInfo = new FileInfo(_config.FxFilePath.Value);
            Directory.CreateDirectory(fileInfo.DirectoryName!);
        }

        _watcher.NotifyFilter = NotifyFilters.LastWrite;
        _watcher.Path = fileInfo.DirectoryName!;
        _watcher.Filter = fileInfo.Name;
        _watcher.EnableRaisingEvents = true;

        if (!fileInfo.Exists)
        {
            using var _ = fileInfo.Create();
        }

        _openFile = fileInfo;
    }

    public string ReadOpenedFile()
    {
        if (_openFile == null)
            throw new InvalidOperationException("No file has been opened yet.");

        using var fileStream = _openFile.Exists ? _openFile.OpenRead() : _openFile.Create();
        using var streamReader = new StreamReader(fileStream);
        return streamReader.ReadToEnd();
    }

    private void OnUpdate(object sender, FileSystemEventArgs args)
    {
        var fileInfo = new FileInfo(args.FullPath);

        if (!fileInfo.Exists)
        {
            _config.FxFilePath.Clear();
            fileInfo = new FileInfo(_config.FxFilePath.Value);
            Directory.CreateDirectory(fileInfo.DirectoryName!);
        }

        using var fileStream = fileInfo.Exists ? fileInfo.OpenRead() : fileInfo.Create();
        using var streamReader = new StreamReader(fileStream);
        var text = streamReader.ReadToEnd();
        Changed?.Invoke(text);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs args)
    {
        if (_openFile == null || _openFile.Exists)
            return;

        using var _ = _openFile.Create();
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _watcher.Dispose();
        }

        _isDisposed = true;
    }

    ~FxFileManager()
    {
        Dispose(false);
    }
}