using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Segments.DataStructures;

public class LabelData
{
    public LabelData(SpriteFont font, string text, Vector2? size = null)
    {
        Font = font;
        Text = text;
        Size = size;
    }

    public SpriteFont Font { get; init; }
    public string Text { get; init; }
    public Vector2? Size { get; init; }
}