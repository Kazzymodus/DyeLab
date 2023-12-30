using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.InputField;

public class TextInputField : InputField<string>
{
    private const int IndentSize = 4;
    private static readonly string Tab = new(' ', IndentSize);

    private static readonly Regex CrlfRegex = new Regex(@"[\n\r]+", RegexOptions.Compiled);
    private readonly bool _isMultiLine;

    private TextInputField(SpriteFont font, bool isMultiLine, float? autoCommitDelay)
        : base(font, autoCommitDelay)
    {
        _isMultiLine = isMultiLine;
    }

    protected override string? ValueToString(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return CrlfRegex.Replace(value, "\r");
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
            return new TextInputField(Font!, _isMultiLine, AutoCommitDelay);
        }
    }

    protected override string Value => Content.ToString();

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
        InsertAtCursor('\r');

        if (lastCharacterIndex < 0)
            return;

        int i;

        for (i = lastCharacterIndex; i > 0; i--)
        {
            if (Content[i] != '\r')
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
}