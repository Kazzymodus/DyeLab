using System.Diagnostics;

namespace DyeLab;

internal static class OS
{
    internal static void Open(string path)
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
}