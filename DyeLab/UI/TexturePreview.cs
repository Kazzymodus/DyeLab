using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DyeLab.UI;

public class TexturePreview : UIElement, IClickable, IScrollable
{
    private const int Margin = 4;
    private Rectangle TextureBounds => new(X + Margin, Y + Margin, Width - Margin * 2, Height - Margin * 2);

    private Texture2D? _texture;
    private readonly SpriteFont _font;
    private readonly Color _color;
    private readonly bool _drawWithEffect;

    private bool _isDragging;
    private Point? _lastMousePosition;
    private Point _offset;

    private const float MaxZoom = 3;
    private float _zoomLevel;

    private float TextureScale => _zoomLevel < 0 ? 1f / (1f + -_zoomLevel) : 1f + _zoomLevel;

    private TexturePreview(Texture2D? texture, SpriteFont font, Color? color, bool drawWithEffect)
    {
        _texture = texture;
        _font = font;
        _color = color ?? Color.White;
        _drawWithEffect = drawWithEffect;
    }

    public void SetTexture(Texture2D? texture)
    {
        _texture = texture;
        _offset = Point.Zero;
    }
    
    protected override void UpdateElement(GameTime gameTime)
    {
        if (_texture == null || !_isDragging)
            return;

        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton != ButtonState.Pressed)
        {
            _isDragging = false;
            _lastMousePosition = null;
            return;
        }

        var mousePosition = new Point(mouseState.X - X, mouseState.Y - Y);

        var scale = TextureScale;
        var textureBounds = TextureBounds;
        var isDraggable = _texture.Width * scale > textureBounds.Width &&
                          _texture.Height * scale > textureBounds.Height;

        if (_lastMousePosition.HasValue && isDraggable)
        {
            var difference = _lastMousePosition.Value - mousePosition;
            if (difference != Point.Zero)
                _offset = new Point(
                    Math.Clamp(_offset.X + difference.X, 0, (int)(_texture.Width * scale) - textureBounds.Width),
                    Math.Clamp(_offset.Y + difference.Y, 0, (int)(_texture.Height * scale) - textureBounds.Height)
                );
        }

        _lastMousePosition = mousePosition;
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawSolid(Vector2.Zero, Width, Height, Color.DarkGray);

        if (_texture == null)
            return;
        
        var textureBounds = TextureBounds;
        var scale = TextureScale;
        var width = _texture.Width * scale < textureBounds.Width
            ? _texture.Width
            : (int)(textureBounds.Width / scale);
        var height = _texture.Height * scale < textureBounds.Height
            ? _texture.Height
            : (int)(textureBounds.Height / scale);

        drawHelper.DrawTexture(_texture, new Vector2(Margin),
            new Rectangle((int)(_offset.X / scale), (int)(_offset.Y / scale), width, height),
            _color, 0f, Vector2.Zero, scale, _drawWithEffect);

        var scaleText = $"{scale:F2}x";
        drawHelper.DrawText(_font, scaleText,
            new Vector2(textureBounds.Width - _font.MeasureString(scaleText).X, Margin), Color.White);
        var charSize = _font.MeasureString("*");
        charSize.Y *= 0.5f;
        if (_offset.X > 0)
            drawHelper.DrawText(_font, "*", new Vector2(Margin, (Height - charSize.Y) * 0.5f), Color.White);
        if (_offset.X < _texture.Width * scale - Width)
            drawHelper.DrawText(_font, "*", new Vector2(textureBounds.Width - charSize.X, (Height - charSize.Y) * 0.5f),
                Color.White);
        if (_offset.Y > 0)
            drawHelper.DrawText(_font, "*", new Vector2((Width - charSize.X) * 0.5f, Margin), Color.White);
        if (_offset.Y < _texture.Height * scale - Height)
            drawHelper.DrawText(_font, "*", new Vector2((Width - charSize.X) * 0.5f, textureBounds.Height - charSize.Y),
                Color.White);
    }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButton button, Point mousePosition)
    {
        _isDragging = true;
        _lastMousePosition = mousePosition;
    }

    public void OnLoseFocus()
    {
    }

    public void OnScroll(int amount)
    {
        if (_texture == null)
            return;
        
        _zoomLevel = Math.Clamp(_zoomLevel + amount, -MaxZoom, MaxZoom);

        var newScale = TextureScale;
        var maxXOffset = Math.Max(0, (int)(_texture.Width * newScale) - Width);
        if (_offset.X > maxXOffset)
            _offset.X = maxXOffset;

        var maxYOffset = Math.Max(0, (int)(_texture.Height * newScale) - Height);
        if (_offset.Y > maxYOffset)
            _offset.Y = maxYOffset;
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<TexturePreview>
    {
        private Texture2D? _texture;
        private SpriteFont? _font;
        private Color? _color;
        private bool _drawWithEffect;

        public Builder()
        {
            AddValidationStep(() => _font != null, "Font has not been set.");
        }

        public Builder SetTexture(Texture2D? texture)
        {
            _texture = texture;
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

        public Builder DrawWithEffect()
        {
            _drawWithEffect = true;
            return this;
        }

        protected override TexturePreview BuildElement()
        {
            return new TexturePreview(_texture!, _font!, _color, _drawWithEffect);
        }
    }
}