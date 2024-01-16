using DyeLab.Configuration;

namespace DyeLab.Editing;

public sealed class FxFileManager : IDisposable
{
    private readonly Config _config;
    private readonly FileSystemWatcher? _watcher;
    private FileInfo? _openFile;

    public string? OpenFilePath => _openFile?.FullName;

    public event EventHandler<FileChangedEventArgs>? Changed;

    private readonly object _fileAccessLock = new();
    private bool _ignoreNextUpdate;
    private bool _queueRefresh;

    private bool _isDisposed;

    public FxFileManager(Config config)
    {
        _config = config;

        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        _watcher = new FileSystemWatcher();
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileDeleted;
        _watcher.Deleted += OnFileDeleted;
    }

    public void Update()
    {
        if (!_queueRefresh)
            return;

        ReloadFile();
        _queueRefresh = false;
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

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            _watcher!.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Path = fileInfo.DirectoryName!;
            _watcher.Filter = fileInfo.Name;
            _watcher.EnableRaisingEvents = true;
        }

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

        try
        {
            lock (_fileAccessLock)
            {
                using var fileStream = _openFile.Exists ? _openFile.OpenRead() : _openFile.Create();
                using var streamReader = new StreamReader(fileStream);
                return streamReader.ReadToEnd();
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return string.Empty;
        }
    }

    public bool SaveToOpenedFile(string text)
    {
        if (_openFile == null)
            throw new InvalidOperationException("No file has been opened yet.");

        _ignoreNextUpdate = true;

        try
        {
            lock (_fileAccessLock)
            {
                using var fileStream = _openFile.Exists ? _openFile.OpenWrite() : _openFile.Create();
                using var steamWriter = new StreamWriter(fileStream);
                fileStream.SetLength(0);
                steamWriter.Write(text);
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }


    private void OnFileChanged(object sender, FileSystemEventArgs args)
    {
        if (_ignoreNextUpdate)
        {
            _ignoreNextUpdate = false;
            return;
        }

        lock (_fileAccessLock)
        {
            if (_openFile?.FullName != args.FullPath)
                _openFile = new FileInfo(args.FullPath);

            _queueRefresh = true;
        }
    }

    private void ReloadFile()
    {
        if (_openFile == null)
            throw new InvalidOperationException("No file has been opened yet.");

        try
        {
            lock (_fileAccessLock)
            {
                if (!_openFile.Exists)
                {
                    _config.FxFilePath.Clear();
                    _openFile = new FileInfo(_config.FxFilePath.Value);
                    Directory.CreateDirectory(_openFile.DirectoryName!);
                }

                using var fileStream = _openFile.Exists ? _openFile.OpenRead() : _openFile.Create();
                using var streamReader = new StreamReader(fileStream);
                var text = streamReader.ReadToEnd();
                Changed?.Invoke(this, new FileChangedEventArgs(text));
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
        }
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

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _watcher?.Dispose();
        }

        _isDisposed = true;
    }

    ~FxFileManager()
    {
        Dispose(false);
    }
}