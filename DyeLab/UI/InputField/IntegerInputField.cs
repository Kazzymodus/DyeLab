using System.Globalization;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.InputField;

public class IntegerInputField : InputField<int>
{
    private IntegerInputField(SpriteFont font, float? autoCommitDelay, bool isReadOnly)
        : base(font, autoCommitDelay, isReadOnly)
    {
    }

    public static Builder New() => new();

    public class Builder : InputFieldBuilder
    {
        protected override InputField<int> BuildElement()
        {
            return new IntegerInputField(Font!, AutoCommitDelay, IsReadOnly);
        }
    }

    protected override int Value =>
        int.TryParse(Content.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
            ? i
            : 0;

    protected override bool IsValidCharacter(char input)
    {
        return char.IsDigit(input);
    }
}