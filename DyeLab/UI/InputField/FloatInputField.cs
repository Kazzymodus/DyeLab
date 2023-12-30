using System.Globalization;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.InputField;

public class FloatInputField : InputField<float>
{
    private FloatInputField(SpriteFont font, float? autoCommitDelay)
        : base(font, autoCommitDelay)
    {
    }

    public static Builder New() => new();

    public class Builder : InputFieldBuilder
    {
        protected override InputField<float> BuildElement()
        {
            return new FloatInputField(Font!, AutoCommitDelay);
        }
    }

    protected override float Value =>
        float.TryParse(Content.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
            ? f
            : 0;

    protected override string? ValueToString(float value)
    {
        return value.ToString("F2").Replace(',', '.');
    }

    protected override bool IsValidCharacter(char input)
    {
        return char.IsDigit(input) || input is '.' or ',';
    }
}