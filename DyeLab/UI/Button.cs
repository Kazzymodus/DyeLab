using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public class Button : UIElement, IClickable
{
    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (!buttons.HasFlag(MouseButtons.LMB))
            return;
    }

    public void OnLoseFocus()
    {
    }
}