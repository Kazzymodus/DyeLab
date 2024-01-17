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
    public ExternalTextureKey[][] ArmorTextures { get; } = new ExternalTextureKey[3][];

    public string? FallbackShader { get; private set; }

    private readonly IList<Texture2D> _terrariaImages;
    private IList<Texture2D> _customImages;

    public IList<Texture2D> Images => _terrariaImages.Concat(_customImages).ToList();

    private const string ContentDirectory = "Content";
    private const string FontsDirectory = "Fonts";
    private const string ImagesDirectory = "Images";

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly FileSystemWatcher? _imageWatcher;

    public event Action<ICollection<Texture2D>>? ImagesUpdated;

    private readonly Dictionary<TerrariaTextureType, string> _terrariaTextureFileMap = new()
    {
        { TerrariaTextureType.Misc, "Images/Misc/" },
        { TerrariaTextureType.PlayerBase, "Images/Player_0_" },
        { TerrariaTextureType.ArmorHead, "Images/Armor_Head_" },
        { TerrariaTextureType.ArmorBody, "Images/Armor/Armor_" },
        { TerrariaTextureType.ArmorLeg, "Images/Armor_Legs_" },
    };

    public AssetManager(ContentManager contentManager, Config config)
    {
        _contentManager = contentManager;
        _config = config;

        CreateDirectories();

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            _imageWatcher = new FileSystemWatcher(ContentDirectory + Path.DirectorySeparatorChar + ImagesDirectory);
            _imageWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _imageWatcher.Filter = "*.png";
            _imageWatcher.EnableRaisingEvents = true;
            _imageWatcher.Created += OnImagesChanged;
            _imageWatcher.Changed += OnImagesChanged;
            _imageWatcher.Renamed += OnImagesChanged;
            _imageWatcher.Deleted += OnImagesChanged;
        }

        GetCustomImages();

        _terrariaImages = new List<Texture2D>();
        _terrariaImages.Add(LoadTerrariaTexture(TerrariaTextureType.Misc, "noise"));
        _terrariaImages.Add(LoadTerrariaTexture(TerrariaTextureType.Misc, "Perlin"));
    }

    private static void CreateDirectories()
    {
        Directory.CreateDirectory(ContentDirectory + Path.DirectorySeparatorChar + ImagesDirectory);
    }

    private void OnImagesChanged(object sender, FileSystemEventArgs e)
    {
        GetCustomImages();
        ImagesUpdated?.Invoke(Images);
    }

    [MemberNotNull(nameof(_customImages))]
    private void GetCustomImages()
    {
        _contentManager.RootDirectory = ContentDirectory + Path.DirectorySeparatorChar + ImagesDirectory;
        var imageFiles = Directory.GetFiles(_contentManager.RootDirectory, "*.png");
        var images = new List<Texture2D>();
        for (var i = 0; i < imageFiles.Length; i++)
        {
            var filePath = imageFiles[i];
            var fileName = imageFiles[i] = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                var image = _contentManager.Load<Texture2D>(fileName);
                images.Add(image);
            }
            catch (Exception e) when (e is ContentLoadException or InvalidOperationException or IOException)
            {
                Console.Write($"Couldn't load file {fileName}: {e}");
            }
        }

        _customImages = images;
    }

    public void LoadAssets()
    {
        LoadArmorIds();
        LoadFallbackShader();
    }

    private void LoadArmorIds()
    {
        LoadIds(0, "headIds");
        LoadIds(1, "bodyIds");
        LoadIds(2, "legIds");

        void LoadIds(int index, string fileName)
        {
            var lines = File.ReadAllLines($"data/{fileName}.txt");
            ArmorTextures[index] = lines.Select(x =>
            {
                var split = x.Split(':');
                return new ExternalTextureKey(split[0], int.Parse(split[1]));
            }).ToArray();
        }
    }

    private void LoadFallbackShader()
    {
        FallbackShader = File.ReadAllText($"data/baseShader.txt");
    }

    public SpriteFont LoadFont(string fontName)
    {
        _contentManager.RootDirectory = ContentDirectory + Path.DirectorySeparatorChar + FontsDirectory;
        return _contentManager.Load<SpriteFont>(fontName);
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

    public Texture2D LoadTerrariaTexture(TerrariaTextureType type, string name)
    {
        var path = _terrariaTextureFileMap[type] + name;
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