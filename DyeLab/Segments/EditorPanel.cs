using System.Diagnostics;
using DyeLab.Editing;
using DyeLab.UI;
using DyeLab.UI.InputField;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Segments;

public static class EditorPanel
{
    public delegate bool CompileDelegate(string shaderText, out string? error);

    public static UIElement Build(Point position, SpriteFont font, FxFileManager fxFileManager,
        CompileDelegate compileDelegate, string? shaderError)
    {
        const int editorWidth = 500;
        const int editorHeight = 375;

        const int errorListPadding = 5;
        
        const int errorListHeight = 100;
        
        const int buttonWidth = 40;
        const int buttonHeight = 20;

        var panel = Panel.New()
            .SetBounds(position.X, position.Y, 0, 0)
            .Build();

        var editor = TextInputField.New()
            .MultiLine()
            .SetFont(font)
            .SetBounds(position.X, position.Y, editorWidth, editorHeight)
            .Build();
        editor.SetValue(fxFileManager.ReadOpenedFile());
        
        var errorField = TextInputField.New()
            .SetFont(font)
            .ReadOnly()
            .SetBounds(position.X, position.Y + editorHeight + buttonHeight + errorListPadding, editorWidth, errorListHeight)
            .Build();
        panel.AddChild(errorField);
        errorField.SetValue(shaderError ?? "No errors.");

        editor.Commit += (_, args) =>
        {
            compileDelegate(args.NewValue, out var error);
            errorField.SetValue(error ?? "No errors!");
        };
        editor.Commit += (_, args) =>
        {
            if (args.IsExternalChange)
                return;
            fxFileManager.SaveToOpenedFile(args.NewValue);
        };
        fxFileManager.Changed += (_, args) => editor.SetValue(args.NewContent);

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var openFileButton = Button.New()
                .SetFont(font)
                .SetLabel("Open")
                .SetBounds(position.X, position.Y + editorHeight, buttonWidth, buttonHeight)
                .Build();
            panel.AddChild(openFileButton);
            openFileButton.Clicked += () =>
            {
                var filePath = fxFileManager.OpenFilePath;
                if (!File.Exists(filePath))
                    return;

                if (!filePath.EndsWith(".fx"))
                    throw new InvalidOperationException("Attempting to open something that isn't an .fx file.");

                try
                {
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = filePath
                    };
                    process.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not open file: {e}");
                }
            };
        }

        panel.AddChild(editor);
        panel.SizeToContents();
        return panel;
    }
}