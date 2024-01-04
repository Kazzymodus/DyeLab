using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Prefabs.DataStructures;

public class InputFieldData
{
    public InputFieldData(SpriteFont font)
    {
        Font = font;
    }

    public SpriteFont Font { get; }
}