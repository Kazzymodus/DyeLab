using DyeLab.Input.Constants;
using Microsoft.Xna.Framework;

namespace DyeLab.UI.Interfaces;

public interface IClickable
{
    bool IsActive { get; }
    
    Rectangle Bounds { get; }
    
    void OnFocus();
    
    void OnClick(MouseButton button, Point mousePosition);

    void OnLoseFocus();
}