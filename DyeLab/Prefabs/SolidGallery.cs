using DyeLab.UI;
using Microsoft.Xna.Framework;

namespace DyeLab.Prefabs;

public static class SolidGallery
{
    public static Panel Build(Point position)
    {
        const int solidCountHorizontal = 3;
        const int solidCountVertical = 3;

        const int spaceBetweenX = Terraria.PlayerWidth + 20;
        const int spaceBetweenY = Terraria.PlayerHeight + 20;

        var solidGallery = Panel.New().SetBounds(position.X, position.Y, 0, 0).Build();
        var colours = new[]
        {
            Color.Black,
            Color.Gray,
            Color.White,
            Color.Red,
            Color.Lime,
            Color.Blue,
            Color.Cyan,
            Color.Magenta,
            Color.Yellow
        };

        for (var i = 0; i < solidCountHorizontal; i++)
        {
            for (var j = 0; j < solidCountVertical; j++)
            {
                solidGallery.AddChild(
                    SolidFrame.New()
                        .SetSolidSize(Terraria.PlayerWidth, Terraria.PlayerSheetHeight)
                        .SetColor(colours[i * solidCountHorizontal + j])
                        .DrawWithEffect()
                        .SetBounds(position.X + j * spaceBetweenX, position.Y + i * spaceBetweenY,
                            Terraria.PlayerWidth, Terraria.PlayerHeight)
                        .Build());
            }
        }

        solidGallery.SizeToContents();
        return solidGallery;
    }
}