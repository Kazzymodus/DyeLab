using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public class Checkbox : UIElement, IClickable
{
    private const float InnerBoxRatio = 0.85f;
    
    public bool Value { get; private set; }

    public event Action<bool>? ValueChanged;
    
    private Checkbox(bool startValue)
    {
        Value = startValue;
    }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (!buttons.HasFlag(MouseButtons.LMB))
            return;
        
        Value = !Value;
        
        ValueChanged?.Invoke(Value);
    }

    public void OnLoseFocus()
    {
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawSolid(Vector2.Zero, Width, Height, Color.DarkGray);
        if (!Value)
            return;

        var difference = new Vector2(Width, Height) * (1 - InnerBoxRatio);
        drawHelper.DrawSolid(difference, (int)Math.Round(Width - difference.X * 2), (int)Math.Round(Height - difference.Y * 2), Color.Gray);
    }

    public static Builder New() => new Builder();

    public class Builder : UIElementBuilder<Checkbox>
    {
        private bool _value;

        public Builder StartTrue()
        {
            _value = true;
            return this;
        }

        protected override Checkbox BuildElement()
        {
            return new Checkbox(_value);
        }
    }
}