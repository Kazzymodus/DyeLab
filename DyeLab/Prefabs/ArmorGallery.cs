using DyeLab.Assets;
using DyeLab.Assets.Constants;
using DyeLab.UI;
using DyeLab.UI.Armor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Prefabs;

public static class ArmorGallery
{
    public static Panel Build(Point position, SpriteFont font, AssetManager assetManager)
    {
        const int armorCountHorizontal = 3;
        const int armorCountVertical = 3;

        const int spaceBetweenX = Terraria.PlayerWidth + 20;
        const int spaceBetweenY = Terraria.PlayerHeight + 20;

        var armorGallery = Panel.New().SetBounds(position.X, position.Y, 0, 0).Build();

        var headSkinTexture = assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 0);
        var bodySkinTexture = assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 3);
        var legSkinTexture = assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 10);

        for (var i = 0; i < armorCountHorizontal; i++)
        {
            for (var j = 0; j < armorCountVertical; j++)
            {
                armorGallery.AddChild(
                    PlayerTile.New()
                        .SetIds(assetManager.ArmorIds[0], assetManager.ArmorIds[1], assetManager.ArmorIds[2])
                        .SetSkinTextures(headSkinTexture, bodySkinTexture, legSkinTexture)
                        .SetTextureLoadingDelegate(assetManager.LoadTerrariaTexture)
                        .SetFont(font)
                        .SetBounds(position.X + j * spaceBetweenX, position.Y + i * spaceBetweenY,
                            Terraria.PlayerWidth, Terraria.PlayerHeight)
                        .Build());
            }
        }

        armorGallery.SetBounds(position.X, position.Y,
            (armorCountHorizontal - 1) * spaceBetweenX + Terraria.PlayerWidth,
            (armorCountVertical - 1) * spaceBetweenY + Terraria.PlayerHeight);

        return armorGallery;
    }
}