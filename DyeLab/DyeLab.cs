using DyeLab.Assets;
using DyeLab.Compiler;
using DyeLab.Configuration;
using DyeLab.Editing;
using DyeLab.Effects;
using DyeLab.Effects.Constants;
using DyeLab.Prefabs;
using DyeLab.Prefabs.DataStructures;
using DyeLab.UI;
using DyeLab.UI.Exceptions;
using DyeLab.UI.InputField;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using ArmorShaderParam = DyeLab.Effects.Constants.TerrariaShaderParameters.Armor;

namespace DyeLab
{
    public class DyeLab : Game
    {
        private const int MaxScreenWidth = 1200;
        private const int MaxScreenHeight = 600;

        private SpriteBatch? _spriteBatch;

        private Effect? _vanillaEffect;
        private Effect? _customEffect;
        private EffectWrapper? _activeEffect;

        private UserInterface? _ui;
        private DrawHelper? _drawHelper;

        private AssetManager? _assetManager;
        private ShaderCompiler? _shaderCompiler;
        private FxFileManager? _fxFileManager;

        private ConfigManager? _configManager;

        private IDictionary<string, EffectParameterWrapper>? _effectParameters;

        private string _shader;

        private float _timeScalar = 1f;
        private float _shaderTime;

        private bool _isDisposed;

        public DyeLab()
        {
            var graphicsDeviceManager = new GraphicsDeviceManager(this);

            graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            graphicsDeviceManager.PreferredBackBufferWidth = MaxScreenWidth;
            graphicsDeviceManager.PreferredBackBufferHeight = MaxScreenHeight;

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _configManager = new ConfigManager();
            _configManager.Load();
            var config = _configManager.Config;

            while (!Directory.Exists(config.TerrariaInstallationPath.Value))
            {
                ShowMessageBox(
                    "Could not find Terraria's installation directory!\n\nPlease update config.json with the correct value.");
                _configManager.Load();
                config = _configManager.Config;
            }

            _assetManager = new AssetManager(Content, config);

            _fxFileManager = new FxFileManager(config);
            _shader = _fxFileManager.OpenFxFile();

            _shaderCompiler = new ShaderCompiler();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _drawHelper = new DrawHelper(_spriteBatch);

            RegisterEffectParameters();

            base.Initialize();
        }

        private void RegisterEffectParameters()
        {
            _effectParameters = new Dictionary<string, EffectParameterWrapper>();
            RegisterParameter(ArmorShaderParam.Color, NewFloat3);
            RegisterParameter(ArmorShaderParam.SecondaryColor, NewFloat3);
            RegisterParameter(ArmorShaderParam.Opacity, NewFloat);
            RegisterParameter(ArmorShaderParam.Saturation, NewFloat);
            RegisterParameter(ArmorShaderParam.Rotation, NewFloat);
            RegisterParameter(ArmorShaderParam.Time, NewFloat);
            RegisterParameter(ArmorShaderParam.Direction, NewFloat);
            RegisterParameter(ArmorShaderParam.TargetPosition, NewFloat2);
            RegisterParameter(ArmorShaderParam.WorldPosition, NewFloat2);

            FloatEffectParameterWrapper NewFloat(string parameterName) => new(parameterName);
            Float2EffectParameterWrapper NewFloat2(string parameterName) => new(parameterName);
            Float3EffectParameterWrapper NewFloat3(string parameterName) => new(parameterName);

            void RegisterParameter(string parameterName, Func<string, EffectParameterWrapper> creator)
            {
                _effectParameters![parameterName] = creator(parameterName);
            }
        }

        protected override void LoadContent()
        {
            try
            {
                _assetManager!.LoadAssets();
                _vanillaEffect = _assetManager.LoadTerrariaEffect("PixelShader");
                _activeEffect = new EffectWrapper(_vanillaEffect);
                _drawHelper!.SetEffect(_activeEffect);
            }
            catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
            {
                ShowMessageBox(
                    $"Some files could not be found:\n\n\"{e.Message}\"\n\nTry reinstalling the application.");
                Exit();
            }
        }

        protected override void BeginRun()
        {
            try
            {
                BuildUI();
            }
            catch (InvalidUIElementException e)
            {
                ShowMessageBox($"Could not initialize the UI:\n\n\"{e.Message}\"");
                Exit();
            }
        }

        private void ShowMessageBox(string message)
        {
            var messageBoxData = new SDL.SDL_MessageBoxData
            {
                flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
                window = nint.Zero,
                title = nameof(DyeLab),
                message = message,
                numbuttons = 1,
                buttons = new[]
                {
                    new SDL.SDL_MessageBoxButtonData
                    {
                        flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
                        buttonid = 0,
                        text = "OK"
                    }
                },
            };
            SDL.SDL_ShowMessageBox(ref messageBoxData, out _);
        }

        private void BuildUI()
        {
            var consolasFont = _assetManager!.LoadFont("Consolas");

            var armorShaderPanel = new Panel();

            var editor = TextInputField.New()
                .MultiLine()
                .SetFont(consolasFont)
                .SetBounds(10, 20, 250, 400)
                .Build();
            editor.SetValue(_shader);
            editor.Commit += CompileAndReload;
            var controlPanel = ControlPanel.Build(new Point(460, 40), consolasFont, _effectParameters!,
                f => _timeScalar = f, new PassSliderData(_activeEffect!),
                new ImageControlData(_assetManager, GraphicsDevice));

            var galleryPosition = new Point(280, 40);
            var galleryTab = TabBar.New()
                .AddTab("Armors", ArmorGallery.Build(galleryPosition, consolasFont, _assetManager))
                .AddTab("Solids", SolidGallery.Build(galleryPosition))
                .SetFont(consolasFont)
                .SetBounds(270, 280, 180, 20)
                .Build();

            armorShaderPanel.AddChild(editor);
            armorShaderPanel.AddChild(galleryTab);
            armorShaderPanel.AddChild(controlPanel);

            _ui = new UserInterface()
                .AddElement(armorShaderPanel)
                .Initialize();
        }

        private void CompileAndReload(string shaderText)
        {
            var isSuccessful = _shaderCompiler!.Compile(shaderText, out var code);
            if (isSuccessful)
                _customEffect = new Effect(GraphicsDevice, code);
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            _ui!.Update(gameTime);

            _shaderTime += (float)gameTime.ElapsedGameTime.TotalSeconds * _timeScalar;
            _shaderTime %= 3600;

            ((FloatEffectParameterWrapper)_effectParameters![ArmorShaderParam.Time]).Set(_shaderTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            foreach (var effectParameter in _effectParameters!.Values)
                effectParameter.Apply(_activeEffect!);

            if (GraphicsDevice.Textures[1] is Texture2D texture)
                _activeEffect!.Parameters[TerrariaShaderParameters.Armor.SecondaryImageSize]
                    .SetValue(new Vector2(texture.Width, texture.Height));
            
            _activeEffect!.Parameters[TerrariaShaderParameters.Armor.Direction].SetValue(1f);
            _activeEffect.Parameters[TerrariaShaderParameters.Armor.TargetPosition].SetValue(Vector2.One);

            _spriteBatch!.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            _ui?.Draw(_drawHelper!);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _fxFileManager?.Dispose();
                _drawHelper?.Dispose();
            }

            base.Dispose(disposing);

            _isDisposed = true;
        }
    }
}