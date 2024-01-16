using System.Diagnostics.CodeAnalysis;
using DyeLab.UI;
using Microsoft.Xna.Framework;

namespace DyeLab.Segments;

public abstract class Segment
{
    public abstract Panel BuildUI(Point position);

    public virtual void Update(GameTime gameTime) {}
}