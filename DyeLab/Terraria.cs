using Microsoft.Xna.Framework;

namespace DyeLab;

public static class Terraria
{
    public const int PlayerWidth = 40;
    public const int PlayerHeight = 56;

    public const int PlayerSheetHeight = 1120;
    
    public static readonly Rectangle PlayerSourceRectangle = new(0, 0, PlayerWidth, PlayerHeight);
    public static readonly Rectangle BodyBackArmSourceRectangle = new(320, 0, PlayerWidth, PlayerHeight);
    public static readonly Rectangle BodyFrontArmSourceRectangle = new(280, 0, PlayerWidth, PlayerHeight);
    public static readonly Rectangle BodySpaulderSourceRectangle = new(0, 56, PlayerWidth, PlayerHeight);
}