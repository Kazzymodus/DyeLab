using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public class Button : UIElement, IClickable
{
    public void OnFocus()
    {
    }

    public void OnClick(MouseButton button, Point mousePosition)
    {
        if (button != MouseButton.LMB)
            return;
    }

    public void OnLoseFocus()
    {
    }
}