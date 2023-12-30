using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public class Checkbox : UIElement, IClickable
{
    public bool Value { get; private set; }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButton button, Point mousePosition)
    {
        if (button != MouseButton.LMB)
            return;
        
        Value = !Value;
    }

    public void OnLoseFocus()
    {
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawSolid(Vector2.Zero, Width, Height, Value ? Color.Green : Color.Red);
    }
}