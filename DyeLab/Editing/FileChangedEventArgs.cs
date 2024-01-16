namespace DyeLab.Editing;

public class FileChangedEventArgs : EventArgs
{
    public string NewContent { get; }

    public FileChangedEventArgs(string newContent)
    {
        NewContent = newContent;
    }
}