namespace DyeLab.Segments.DataStructures;

public class EditorMethods
{
    public delegate bool CompileAndLoadFunc(string shaderText, out string? error);

    public EditorMethods(CompileAndLoadFunc compileAndLoad, Action<string> export)
    {
        CompileAndLoad = compileAndLoad;
        Export = export;
    }
    
    public CompileAndLoadFunc CompileAndLoad { get; }
    public Action<string> Export { get; }
}