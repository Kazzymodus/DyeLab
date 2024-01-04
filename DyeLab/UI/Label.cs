using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI;

public class Label : UIElement
{
    private string _text;
    private readonly SpriteFont _font;
    private readonly Color _color;

    private string _drawText = string.Empty;

    private Label(string text, SpriteFont font, Color color)
    {
        _font = font;
        _color = color;
        _text = text;
    }

    public void SetText(string text)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
        CalculateDrawText();
    }

    public override void SetBounds(int x, int y, int width, int height)
    {
        base.SetBounds(x, y, width, height);
        
        CalculateDrawText();
    }

    public void CalculateDrawText()
    {
        var boundsInCharacters = new Vector2(Width, Height) / _font.MeasureString("*");
        if (boundsInCharacters.X == 0 || boundsInCharacters.Y == 0)
        {
            _drawText = string.Empty;
            return;
        }

        var width = (int)boundsInCharacters.X;
        if (_text.Length <= width)
        {
            _drawText = _text;
            return;
        }

        var baseLength = Math.Max(width - 3, 0);
        if (baseLength == 0)
        {
            _drawText = new string('.', Math.Min(width, 3));
            return;
        }

        _drawText = _text[..baseLength] + new string('.', Math.Min(width - baseLength, 3));
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawText(_font, _drawText, Vector2.Zero, _color);
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<Label>
    {
        private string? _text;
        private SpriteFont? _font;
        private Color? _color;

        public Builder()
        {
            AddValidationStep(() => _font != null, "Font has not been set.");
            AddValidationStep(() => !string.IsNullOrWhiteSpace(_text), "Text has not been set.");
        }

        public Builder SetText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text must not be null or whitespace.");

            _text = text;
            return this;
        }

        public Builder SetFont(SpriteFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            return this;
        }

        public Builder SetColor(Color color)
        {
            _color = color;
            return this;
        }

        protected override Label BuildElement()
        {
            return new Label(_text!, _font!, _color ?? Color.White);
        }
    }
}