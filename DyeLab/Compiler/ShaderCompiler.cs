using System.Diagnostics.CodeAnalysis;

namespace DyeLab.Compiler;

public class ShaderCompiler
{
    private const string CompiledFilePath = "Content/compiler/";
    private const string CompiledFileName = "effect";
    private const string FxExtension = ".fx";
    private const string FxbExtension = ".fxb";

    public bool Compile(string shaderText, [NotNullWhen(true)]out byte[]? code)
    {
        try
        {
            // var sourceFile = $"{CompiledFilePath}{CompiledFileName}{FxExtension}";
            // var targetFile = $"{CompiledFilePath}{CompiledFileName}{FxbExtension}";
            //

            return D3DCompiler.Compile(CompileCode(shaderText), out code);

            //
            // if (!Directory.Exists(CompiledFilePath))
            //     Directory.CreateDirectory(CompiledFilePath);
            //
            // if (!File.Exists("fxc.exe"))
            // {
            //     throw new Exception("No fxc.exe");
            // }
            //
            // using (var fs = new FileStream(sourceFile, FileMode.Create))
            // {
            //     var fullShaderCode = CompileCode(shaderText);
            //     var data = Encoding.ASCII.GetBytes(fullShaderCode);
            //     fs.Write(data, 0, data.Length);
            // }
            //
            // using (var fxc = new Process())
            // {
            //     fxc.StartInfo.UseShellExecute = false;
            //     fxc.StartInfo.RedirectStandardError = true;
            //     fxc.StartInfo.FileName = "fxc.exe";
            //     fxc.StartInfo.CreateNoWindow = true;
            //     fxc.StartInfo.Arguments = $"/T fx_2_0 {sourceFile} /Fo {targetFile}";
            //     fxc.Start();
            //
            //     var error = fxc.StandardError.ReadToEnd();
            //     Console.WriteLine(error);
            //
            //     if (string.IsNullOrEmpty(error))
            //         isSuccessful = true;
            //
            //     if (!fxc.WaitForExit(5000))
            //     {
            //         throw new Exception("fxc.exe failed");
            //     }
            // }
            //
            // if (File.Exists(sourceFile))
            //     File.Delete(sourceFile);
            // else
            // {
            //     throw new Exception(".fx is gone");
            // }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            code = null;
            return false;
        }
    }

    private string CompileCode(string pass)
    {
        return @$"{ShaderBoilerplate.DefaultParameters}

{ShaderBoilerplate.DefaultPassSignature}
{{
    {pass}
}}

{ShaderBoilerplate.DefaultTechniqueDefinition}";
    }
}