using DyeLab.Editing;
using DyeLab.Segments.DataStructures;
using DyeLab.UI;
using DyeLab.UI.InputField;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Segments;

public static class EditorPanel
{
    public static UIElement Build(Point position, SpriteFont font, FxFileManager fxFileManager,
        EditorMethods methods, string? shaderError)
    {
        const int editorWidth = 500;
        const int editorHeight = 375;

        const int errorListPadding = 5;
        
        const int errorListHeight = 100;
        
        const int buttonWidth = 80;
        const int buttonHeight = 20;
        const int buttonPadding = 10;

        const string noErrorsText = "No errors!";

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
        errorField.SetValue(shaderError ?? noErrorsText);

        editor.Commit += (_, args) =>
        {
            methods.CompileAndLoad(args.NewValue, out var error);
            errorField.SetValue(error ?? noErrorsText);
        };
        editor.Commit += (_, args) =>
        {
            if (args.IsExternalChange)
                return;
            fxFileManager.SaveToOpenedFile(args.NewValue);
        };
        fxFileManager.Changed += (_, args) => editor.SetValue(args.NewContent);

        var buttonX = 0;
        
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var openInputFileButton = Button.New()
                .SetFont(font)
                .SetLabel("Open")
                .SetBounds(position.X, position.Y + editorHeight, buttonWidth, buttonHeight)
                .Build();
            panel.AddChild(openInputFileButton);
            openInputFileButton.Clicked += fxFileManager.OpenInputFile;
            buttonX += buttonWidth + buttonPadding;
            
            var openOutputDirectoryButton = Button.New()
                .SetFont(font)
                .SetLabel("Output")
                .SetBounds(position.X + buttonX, position.Y + editorHeight, buttonWidth, buttonHeight)
                .Build();
            panel.AddChild(openOutputDirectoryButton);
            openOutputDirectoryButton.Clicked += fxFileManager.OpenOutputDirectory;
            buttonX += buttonWidth + buttonPadding;
        }
        
        var exportButton = Button.New()
            .SetFont(font)
            .SetLabel("Compile")
            .SetBounds(position.X + buttonX, position.Y + editorHeight, buttonWidth, buttonHeight)
            .Build();
        panel.AddChild(exportButton);
        exportButton.Clicked += () =>
        {
            methods.Export(editor.Value);
        };

        panel.AddChild(editor);
        panel.SizeToContents();
        return panel;
    }
}