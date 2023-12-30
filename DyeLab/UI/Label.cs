using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI;

public class Label : UIElement
{
    private string _text;
    private readonly SpriteFont _font;
    private readonly Color _color;
    
    private Label(string text, SpriteFont font, Color color)
    {
        _text = text;
        _font = font;
        _color = color;
    }

    public void SetText(string text)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawText(_font, _text, Vector2.Zero, _color);
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