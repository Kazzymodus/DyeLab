using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Segments.DataStructures;

public class InputFieldData
{
    public InputFieldData(SpriteFont font)
    {
        Font = font;
    }

    public SpriteFont Font { get; }
}