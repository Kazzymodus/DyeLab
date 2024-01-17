using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.InputField;

public partial class TextInputField : InputField<string>
{
    private const int IndentSize = 4;
    private static readonly string Tab = new(' ', IndentSize);

    private static readonly Regex NewLineRegex = CrlfRegex();
    private readonly bool _isMultiLine;

    private TextInputField(SpriteFont font, bool isMultiLine, float? autoCommitDelay, bool isReadOnly)
        : base(font, autoCommitDelay, isReadOnly)
    {
        _isMultiLine = isMultiLine;
    }

    protected override string ValueToString(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var crlfStripped = NewLineRegex.Replace(value, "\n");
        return crlfStripped.Replace("\t", Tab);
    }

    public static Builder New() => new();

    public class Builder : InputFieldBuilder
    {
        private bool _isMultiLine;

        public Builder MultiLine()
        {
            _isMultiLine = true;
            return this;
        }

        protected override InputField<string> BuildElement()
        {
            return new TextInputField(Font!, _isMultiLine, AutoCommitDelay, IsReadOnly);
        }
    }

    public override string Value => Content.ToString();

    protected override bool IsValidCharacter(char input)
    {
        return true;
    }

    protected override void HandleReturn()
    {
        if (!_isMultiLine)
        {
            base.HandleReturn();
            return;
        }

        var lastCharacterIndex = CursorPosition - 1;
        InsertAtCursor(WhiteLineChar);

        if (lastCharacterIndex < 0)
            return;

        int i;

        for (i = lastCharacterIndex; i > 0; i--)
        {
            if (Content[i] != WhiteLineChar)
                continue;

            i++;
            break;
        }

        while (i <= lastCharacterIndex && Content[i++] == ' ')
        {
            InsertAtCursor(' ');
        }
    }

    protected override void HandleTab()
    {
        if (_isMultiLine)
            InsertAtCursor(Tab);
    }

    [GeneratedRegex("\r\n?", RegexOptions.Compiled)]
    private static partial Regex CrlfRegex();
}