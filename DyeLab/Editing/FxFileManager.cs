using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DyeLab.Configuration;

namespace DyeLab.Editing;

public sealed class FxFileManager : IDisposable
{
    private readonly Config _config;
    private readonly FileSystemWatcher? _watcher;
    private FileInfo? _inputFile;
    private FileInfo? _compiledFile;
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

        RefreshFile();
        _queueRefresh = false;
    }

    public void LoadFxFile()
    {
        var path = _config.FxFilePath.Value;
        var rawFileInfo = new FileInfo(path);

        if (!rawFileInfo.Exists)
        {
            _config.FxFilePath.Clear();
            rawFileInfo = new FileInfo(_config.FxFilePath.Value);
            Directory.CreateDirectory(rawFileInfo.DirectoryName!);
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            _watcher!.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Path = rawFileInfo.DirectoryName!;
            _watcher.Filter = rawFileInfo.Name;
            _watcher.EnableRaisingEvents = true;
        }

        if (!rawFileInfo.Exists)
        {
            using var _ = rawFileInfo.Create();
        }

        _inputFile = rawFileInfo;

        var fileName = !string.IsNullOrEmpty(_config.CompiledFileName.Value)
            ? _config.CompiledFileName.Value
            : Path.GetFileNameWithoutExtension(_inputFile.FullName);
        _compiledFile = new FileInfo(_config.OutputDirectory.Value + Path.DirectorySeparatorChar + fileName + ".xnb");
    }

    public string ReadOpenedFile()
    {
        ThrowIfNoInputFileOpened();

        try
        {
            lock (_fileAccessLock)
            {
                using var fileStream = _inputFile.Exists ? _inputFile.OpenRead() : _inputFile.Create();
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
        ThrowIfNoInputFileOpened();

        _ignoreNextUpdate = true;

        try
        {
            lock (_fileAccessLock)
            {
                using var fileStream = _inputFile.Exists ? _inputFile.OpenWrite() : _inputFile.Create();
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

    public FileInfo? ExportCompiledToFile(byte[] code)
    {
        ThrowIfCompiledFilePathNotSet();

        Directory.CreateDirectory(_compiledFile.DirectoryName!);

        try
        {
            using var fileStream = _compiledFile.Exists ? _compiledFile.OpenWrite() : _compiledFile.Create();
            using var steamWriter = new StreamWriter(fileStream);
            fileStream.SetLength(0);
            steamWriter.Write(code);
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return null;
        }

        return _compiledFile.Exists ? _compiledFile : null;
    }

    public void OpenOutputDirectory()
    {
        Directory.CreateDirectory(_config.OutputDirectory.Value);
        Open(_config.OutputDirectory.Value + Path.DirectorySeparatorChar);
    }

    public void OpenInputFile()
    {
        ThrowIfNoInputFileOpened();

        if (!_inputFile.Exists)
            throw new InvalidOperationException("The input file does not exist.");

        if (!_inputFile.Name.EndsWith(".fx"))
        {
            Console.WriteLine("WARNING: The input file is not an .fx file!");
            throw new InvalidOperationException("Attempting to open a non-FX file.");
        }

        Open(_inputFile.FullName);
    }

    private static void Open(string path)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            throw new PlatformNotSupportedException();

        var allowedFileTypes = new[] { ".fx" };

        if (!Path.Exists(path))
            throw new ArgumentException("The given directory or file does not exist.");

        if (!Path.EndsInDirectorySeparator(path) && allowedFileTypes.All(x => !path.EndsWith(x)))
            throw new ArgumentException("The path is not a directory and is not an allowed file type.");

        try
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = path
            };
            process.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not open file: {e}");
        }
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
            if (_inputFile?.FullName != args.FullPath)
                _inputFile = new FileInfo(args.FullPath);

            _queueRefresh = true;
        }
    }

    private void RefreshFile()
    {
        ThrowIfNoInputFileOpened();

        try
        {
            lock (_fileAccessLock)
            {
                if (!_inputFile.Exists)
                {
                    _config.FxFilePath.Clear();
                    _inputFile = new FileInfo(_config.FxFilePath.Value);
                    Directory.CreateDirectory(_inputFile.DirectoryName!);
                }

                using var fileStream = _inputFile.Exists ? _inputFile.OpenRead() : _inputFile.Create();
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
        if (_inputFile == null || _inputFile.Exists)
            return;

        using var _ = _inputFile.Create();
    }

    [MemberNotNull(nameof(_inputFile))]
    private void ThrowIfNoInputFileOpened()
    {
        if (_inputFile == null)
            throw new InvalidOperationException("No file has been opened yet.");
    }

    [MemberNotNull(nameof(_compiledFile))]
    private void ThrowIfCompiledFilePathNotSet()
    {
        if (_compiledFile == null)
            throw new InvalidOperationException("Compiled file path has not been set. Has an input file been opened?");
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