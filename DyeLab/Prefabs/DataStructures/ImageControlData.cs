using DyeLab.Assets;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Prefabs.DataStructures;

public class ImageControlData
{
    public ImageControlData(AssetManager assetManager, GraphicsDevice graphicsDevice)
    {
        AssetManager = assetManager;
        GraphicsDevice = graphicsDevice;
    }
    
    public AssetManager AssetManager { get; }
    public GraphicsDevice GraphicsDevice { get; }
}