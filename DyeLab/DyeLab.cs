using System.Diagnostics.CodeAnalysis;
using DyeLab.Assets;
using DyeLab.Compiler;
using DyeLab.Configuration;
using DyeLab.Editing;
using DyeLab.Effects;
using DyeLab.Effects.Constants;
using DyeLab.Segments;
using DyeLab.Segments.DataStructures;
using DyeLab.UI;
using DyeLab.UI.Exceptions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using ArmorShaderParam = DyeLab.Effects.Constants.TerrariaShaderParameters.Armor;

namespace DyeLab
{
    public class DyeLab : Game
    {
        private const int MaxScreenWidth = 1200;
        private const int MaxScreenHeight = 540;

        private SpriteBatch? _spriteBatch;

        private readonly IDictionary<EffectType, Effect> _effects = new Dictionary<EffectType, Effect>();
        private EffectType _activeEffectType = EffectType.None;
        private EffectWrapper? _activeEffect;

        private readonly ICollection<Segment> _segments = new List<Segment>();
        private UserInterface? _ui;
        private DrawHelper? _drawHelper;

        private AssetManager? _assetManager;
        private FxFileManager? _fxFileManager;

        private ConfigManager? _configManager;

        private IDictionary<string, EffectParameterWrapper>? _effectParameters;

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
            _fxFileManager.OpenFxFile();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

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
            }
            catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
            {
                ShowMessageBox(
                    $"Some files could not be found:\n\n\"{e.Message}\"\n\nTry reinstalling the application.");
                Exit();
            }

            if (!ShaderCompiler.Compile(_assetManager!.FallbackShader!, out var fallbackShader, out _))
            {
                ShowMessageBox($"The fallback shader is corrupted.\n\nTry reinstalling the application.");
                Exit();
            }

            var fallbackEffect = new Effect(GraphicsDevice, fallbackShader);
            _effects[EffectType.Fallback] = fallbackEffect;
            var vanillaEffect = _assetManager.LoadTerrariaEffect("PixelShader");
            _effects[EffectType.Vanilla] = vanillaEffect;
        }

        protected override void BeginRun()
        {
            var effectCode = _fxFileManager!.ReadOpenedFile();

            _activeEffectType = CompileAndReload(effectCode, out var error) ? EffectType.Editor : EffectType.Fallback;
            _activeEffect = new EffectWrapper(_effects[_activeEffectType]);
            _drawHelper = new DrawHelper(_spriteBatch!, _activeEffect);

            try
            {
                BuildUI(error);
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

        private void BuildUI(string? shaderError)
        {
            const int editorX = 10;
            const int editorY = 20;
            const int editorWidth = 500;
            const int editorHeight = 480;

            const int galleryX = 20;
            const int galleryY = 20;
            const int galleryWidth = 180;

            const int controlPanelX = 60;
            const int controlPanelY = 20;

            var x = 0;
            var consolasFont = _assetManager!.LoadFont("Consolas");

            var armorShaderPanel = new Panel();

            var editorPanel = EditorPanel.Build(new Point(editorX, editorY), consolasFont, _fxFileManager!,
                CompileAndReload, shaderError);

            x += editorX + editorWidth;

            var galleryPosition = new Point(editorWidth + galleryX, galleryY);
            var armorGallery = new PlayerFrameGallery(consolasFont, _assetManager);
            _segments.Add(armorGallery);
            var armorGalleryPanel = armorGallery.BuildUI(galleryPosition);

            x += galleryX + galleryWidth;

            var controlPanel = ControlPanel.Build(new Point(x + controlPanelX, controlPanelY), consolasFont,
                _effectParameters!,
                f => _timeScalar = f, new PassSliderData(_activeEffect!, SetActiveEffect),
                new ImageControlData(_assetManager, GraphicsDevice));

            armorShaderPanel.AddChild(editorPanel);

            armorShaderPanel.AddChild(armorGalleryPanel);
            armorShaderPanel.AddChild(controlPanel);

            _ui = new UserInterface()
                .AddElement(armorShaderPanel)
                .Initialize();
        }

        private void SetActiveEffect(EffectType effectType)
        {
            if (_effects.TryGetValue(effectType, out var effect))
            {
                _activeEffect!.SetEffect(effect);
                return;
            }

            _activeEffect!.SetEffect(_effects[EffectType.Fallback]);
        }

        private bool CompileAndReload(string shaderText, [NotNullWhen(false)] out string? error)
        {
            if (!ShaderCompiler.Compile(shaderText, out var code, out error) || error != null)
                return false;

            if (_effects.TryGetValue(EffectType.Editor, out var editorShader))
                editorShader.Dispose();

            var effect = new Effect(GraphicsDevice, code);
            _effects[EffectType.Editor] = effect;
            if (_activeEffectType == EffectType.Fallback)
                _activeEffectType = EffectType.Editor;

            if (_activeEffectType == EffectType.Editor)
                _activeEffect!.SetEffect(effect);

            return true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            foreach (var segment in _segments)
                segment.Update(gameTime);

            _ui!.Update(gameTime);
            _fxFileManager?.Update();

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