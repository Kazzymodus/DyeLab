using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DyeLab.Compiler;

public static class D3DCompiler
{
    public static bool Compile(string code, [NotNullWhen(true)] out byte[]? effectCode)
    {
        effectCode = null;
        _ = D3DCompile(code, code.Length, null, nint.Zero, nint.Zero, null, "fx_2_0", 0, 0, out var shader, out var errors);
        if (errors != null)
            Console.WriteLine(Marshal.PtrToStringAnsi(errors.GetBufferPointer()));

        if (shader == null) return false;

        var length = shader.GetBufferSize();
        effectCode = new byte[length];
        Marshal.Copy(shader.GetBufferPointer(), effectCode, 0, length);
        return true;
    }

    [PreserveSig]
    [DllImport("D3DCompiler_43.dll", CharSet = CharSet.Auto)]
    private static extern int D3DCompile(
        [MarshalAs(UnmanagedType.LPStr)] string pSrcData,
        int dataLen,
        [MarshalAs(UnmanagedType.LPStr)] string? pSourceName,
        nint pDefines,
        nint includes,
        [MarshalAs(UnmanagedType.LPStr)] string? pEntryPoint,
        [MarshalAs(UnmanagedType.LPStr)] string pTarget,
        int flags,
        int flags2,
        out ID3DxBlob? ppShader,
        out ID3DxBlob? ppErrorMsgs);
    
    [ComImport]
    [Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ID3DxBlob
    {
        [PreserveSig]
        nint GetBufferPointer();

        [PreserveSig]
        int GetBufferSize();
    }
}