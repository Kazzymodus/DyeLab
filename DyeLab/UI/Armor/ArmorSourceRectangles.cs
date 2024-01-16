using DyeLab.UI.Armor.Constants;
using Microsoft.Xna.Framework;

namespace DyeLab.UI.Armor;

public class ArmorSourceRectangles
{
    public Rectangle Legacy { get; private set; }
    public Rectangle Torso { get; private set; }
    public Rectangle FrontArm { get; private set; }
    public Rectangle BackArm { get; private set; }
    public Rectangle Shoulder { get; private set; }
    public ShoulderDrawMode ShoulderDrawMode { get; private set; }

    public ArmorSourceRectangles()
    {
        Calculate(0);
    }

    public void Calculate(int frame)
    {
        Legacy = CalculateFrame(0, frame);
        Torso = CalculateFrame(frame == 5 ? 1 : 0, 0);

        var armX = frame switch
        {
            < 5 => 2 + frame, // Swing
            5 => 2, // 
            6 => 3,
            7 or 8 or 9 or 10 => 4,
            11 or 12 or 13 => 3,
            14 => 5,
            15 or 16 => 6,
            17 => 5,
            18 or 19 => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, null)
        };
        var armY = frame >= 5 ? 1 : 0;
        FrontArm = CalculateFrame(armX, armY);
        BackArm = CalculateFrame(armX, armY + 2);
        Shoulder = CalculateFrame(0, 1);
        ShoulderDrawMode = frame switch
        {
            1 or 2 => ShoulderDrawMode.Under,
            5 => ShoulderDrawMode.None,
            _ => ShoulderDrawMode.Over
        };
    }

    private static Rectangle CalculateFrame(int x, int y) => new(x * Terraria.PlayerWidth,
        y * Terraria.PlayerHeight, Terraria.PlayerWidth, Terraria.PlayerHeight);
}