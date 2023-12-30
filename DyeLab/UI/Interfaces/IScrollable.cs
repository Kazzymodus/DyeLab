using Microsoft.Xna.Framework;

namespace DyeLab.UI.Interfaces;

public interface IScrollable
{
    bool IsActive { get; }
    Rectangle Bounds { get; }
    
    void OnScroll(int amount);
}