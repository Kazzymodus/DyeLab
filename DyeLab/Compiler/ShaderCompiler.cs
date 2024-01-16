using System.Diagnostics.CodeAnalysis;

namespace DyeLab.Compiler;

public class ShaderCompiler
{
    public static bool Compile(string shaderText, [NotNullWhen(true)]out byte[]? code, [NotNullWhen(false)]out string? error)
    {
        try
        {
            return D3DCompiler.Compile(shaderText, out code, out error);
        }
        catch (Exception e)
        {
            code = null;
            error = e.Message;
            return false;
        }
    }
}