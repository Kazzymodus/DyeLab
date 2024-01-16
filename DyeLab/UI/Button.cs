using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI;

public class Button : UIElement, IClickable
{
    private readonly SpriteFont? _font;
    private readonly string? _label;

    private Button(SpriteFont? font, string? label)
    {
        if (label != null && font == null)
            throw new ArgumentNullException(nameof(font), "Font must not be null if a label is specified.");

        _font = font;
        _label = label;
    }

    public event Action? Clicked;

    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (!buttons.HasFlag(MouseButtons.LMB))
            return;

        Clicked?.Invoke();
    }

    public void OnLoseFocus()
    {
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawSolid(Vector2.Zero, Width, Height, new Color(80, 80, 80, 255));
        if (_font == null || string.IsNullOrEmpty(_label))
            return;
        var buttonCenter = new Vector2(Width * 0.5f, Height * 0.5f);
        var textCenter = _font.MeasureString(_label) * 0.5f;
        drawHelper.DrawText(_font, _label, buttonCenter - textCenter, Color.White);
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<Button>
    {
        private SpriteFont? _font;
        private string? _label;

        public Builder()
        {
            AddValidationStep(() => _label == null || _font != null, "Font must be set if a label is specified.");
        }

        public Builder SetFont(SpriteFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            return this;
        }

        public Builder SetLabel(string label)
        {
            _label = label;
            return this;
        }

        protected override Button BuildElement()
        {
            return new Button(_font, _label);
        }
    }
}