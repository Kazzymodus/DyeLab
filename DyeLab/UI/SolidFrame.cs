using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public class SolidFrame : UIElement
{
    private readonly int _solidWidth;
    private readonly int _solidHeight;
    private readonly Color _color;
    private readonly bool _drawWithEffect;
    private Rectangle _sourceRectangle;

    private SolidFrame(int solidWidth, int solidHeight, Color? color, bool drawWithEffect)
    {
        ThrowIfZeroOrNegative(solidWidth);
        ThrowIfZeroOrNegative(solidHeight);

        _solidWidth = solidWidth;
        _solidHeight = solidHeight;
        _color = color ?? Color.White;
        _drawWithEffect = drawWithEffect;
        _sourceRectangle = new Rectangle(0, 0, Width, Height);

        void ThrowIfZeroOrNegative(int property, [CallerArgumentExpression("property")] string? propertyName = null)
        {
            if (property <= 0)
                throw new ArgumentOutOfRangeException(propertyName, property,
                    $"{propertyName} must be larger than zero.");
        }
    }

    public override void SetBounds(int x, int y, int width, int height)
    {
        base.SetBounds(x, y, width, height);

        _sourceRectangle.Width = width;
        _sourceRectangle.Height = height;
    }

    public void SetSourceRectanglePosition(int x, int y)
    {
        _sourceRectangle = new Rectangle(x, y, Width, Height);
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawColoredSolid(Vector2.Zero, _solidWidth, _solidHeight, _sourceRectangle, _color, _drawWithEffect);
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<SolidFrame>
    {
        private int _solidWidth;
        private int _solidHeight;
        private Color? _color;
        private bool _drawWithEffect;

        public Builder()
        {
            AddValidationStep(() => _solidWidth >= 0, "Width has not been set.");
            AddValidationStep(() => _solidWidth >= 0, "Height has not been set.");
        }

        public Builder SetSolidSize(int width, int height)
        {
            ThrowIfZeroOrNegative(width);
            ThrowIfZeroOrNegative(height);

            _solidWidth = width;
            _solidHeight = height;

            return this;

            void ThrowIfZeroOrNegative(int property, [CallerArgumentExpression("property")] string? propertyName = null)
            {
                if (property <= 0)
                    throw new ArgumentOutOfRangeException(propertyName, property,
                        $"{propertyName} must be larger than zero.");
            }
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

        protected override SolidFrame BuildElement()
        {
            return new SolidFrame(_solidWidth, _solidHeight, _color, _drawWithEffect);
        }
    }
}