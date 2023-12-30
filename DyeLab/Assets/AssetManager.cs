using System.Diagnostics.CodeAnalysis;
using DyeLab.Assets.Constants;
using DyeLab.Configuration;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Assets;

public class AssetManager
{
    private readonly ContentManager _contentManager;
    private readonly Config _config;
    public string[][] ArmorIds { get; } = new string[3][];

    private const string ContentDirectory = "Content";
    private const string FontsDirectory = "Fonts";
    private const string ImagesDirectory = "Images";

    private readonly Dictionary<TerrariaTextureType, string> _terrariaTextureFileMap = new()
    {
        { TerrariaTextureType.PlayerBase, "Images/Player_0_" },
        { TerrariaTextureType.ArmorHead, "Images/Armor_Head_" },
        { TerrariaTextureType.ArmorBody, "Images/Armor/Armor_" },
        { TerrariaTextureType.ArmorLeg, "Images/Armor_Legs_" },
    };

    public AssetManager(ContentManager contentManager, Config config)
    {
        _contentManager = contentManager;
        _config = config;
    }

    public void CreateDirectories()
    {
        _contentManager.RootDirectory = "Content";
        Directory.CreateDirectory(_contentManager.RootDirectory + Path.DirectorySeparatorChar + ImagesDirectory);
    }

    public static string[] GetFiles(string directory)
    {
        var files = Directory.GetFiles("Content" + Path.DirectorySeparatorChar + directory);
        for (var i = 0; i < files.Length; i++)
        {
            var filePath = files[i];
            files[i] = Path.GetFileNameWithoutExtension(filePath);
        }

        return files;
    }

    public void LoadAssets()
    {
        LoadArmorIds();
    }

    private void LoadArmorIds()
    {
        LoadIds(0, "headIds");
        LoadIds(1, "bodyIds");
        LoadIds(2, "legIds");

        void LoadIds(int index, string fileName)
        {
            ArmorIds[index] = File.ReadAllLines($"data/{fileName}.txt");
        }
    }

    public SpriteFont LoadFont(string fontName)
    {
        _contentManager.RootDirectory = ContentDirectory + Path.DirectorySeparatorChar + FontsDirectory;
        return _contentManager.Load<SpriteFont>(fontName);
    }
    
    public Texture2D LoadImage(string imageName)
    {
        _contentManager.RootDirectory = ContentDirectory + Path.DirectorySeparatorChar + ImagesDirectory;
        return _contentManager.Load<Texture2D>(imageName);
    }


    public bool TryLoadImage(string imageName, [NotNullWhen(true)] out Texture2D? texture)
    {
        texture = null;

        if (string.IsNullOrEmpty(imageName))
            return false;

        _contentManager.RootDirectory = ContentDirectory + Path.DirectorySeparatorChar + ImagesDirectory;
        if (!File.Exists(_contentManager.RootDirectory + Path.DirectorySeparatorChar + imageName + ".png"))
            return false;

        texture = _contentManager.Load<Texture2D>(imageName);
        return true;
    }

    public bool TryLoadExternalImage(string path, [NotNullWhen(true)] out Texture2D? texture)
    {
        texture = null;

        if (string.IsNullOrEmpty(path))
            return false;

        if (!Path.IsPathFullyQualified(path))
            return false;

        if (!File.Exists(path))
            return false;

        if (Path.GetExtension(path) != ".png")
            return false;

        _contentManager.RootDirectory = Path.GetDirectoryName(path);

        texture = _contentManager.Load<Texture2D>(Path.GetFileNameWithoutExtension(path));
        return true;
    }

    public Effect LoadTerrariaEffect(string name)
    {
        SetContentDirectoryToTerraria();
        return _contentManager.Load<Effect>(name);
    }

    public Texture2D LoadTerrariaTexture(TerrariaTextureType type, int id)
    {
        var path = _terrariaTextureFileMap[type] + id;
        return LoadTerrariaTexture(path);
    }

    public Texture2D LoadTerrariaTexture(string path)
    {
        SetContentDirectoryToTerraria();
        return _contentManager.Load<Texture2D>(path);
    }

    private void SetContentDirectoryToTerraria()
    {
        _contentManager.RootDirectory =
            _config.TerrariaInstallationPath.Value + Path.DirectorySeparatorChar + ContentDirectory;
    }
}