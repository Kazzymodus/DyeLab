using DyeLab.Assets;
using DyeLab.Assets.Constants;
using DyeLab.Compiler;
using DyeLab.Configuration;
using DyeLab.Editing;
using DyeLab.Effects;
using DyeLab.Effects.Constants;
using DyeLab.UI;
using DyeLab.UI.Armor;
using DyeLab.UI.Exceptions;
using DyeLab.UI.InputField;
using DyeLab.UI.ScrollableList;
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

        private Effect? _effect;

        private UserInterface? _ui;
        private DrawHelper? _drawHelper;

        private AssetManager? _assetManager;
        private ShaderCompiler? _shaderCompiler;
        private FxFileManager? _fxFileManager;

        private ConfigManager? _configManager;

        private IDictionary<string, EffectParameterWrapper>? _effectParameters;

        private bool _terrariaFound;
        private Texture2D _noise;

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

            _assetManager = new AssetManager(Content, config);
            _assetManager.CreateDirectories();

            _fxFileManager = new FxFileManager(config);
            _shader = _fxFileManager.OpenFxFile();

            _shaderCompiler = new ShaderCompiler();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _drawHelper = new DrawHelper(_spriteBatch);

            RegisterEffectParameters();

            _terrariaFound = Directory.Exists(config.TerrariaInstallationPath.Value) &&
                             File.Exists(config.TerrariaInstallationPath.Value + Path.DirectorySeparatorChar +
                                         "Terraria.exe");

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
                _effect = _assetManager.LoadTerrariaEffect("PixelShader");
                _noise = _assetManager.LoadTerrariaTexture("Images/Misc/noise");
                GraphicsDevice.Textures[1] = _noise;
                _drawHelper!.SetEffect(_effect);
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
            var controlPanel = BuildControlPanel(new Point(460, 40), consolasFont);

            var galleryPosition = new Point(280, 40);
            var galleryTab = TabBar.New()
                .AddTab("Armors", BuildArmorGalleryPanel(galleryPosition, consolasFont))
                .AddTab("Solids", BuildSolidGalleryPanel(galleryPosition))
                .SetFont(consolasFont)
                .SetBounds(270, 280, 180, 20)
                .Build();

            armorShaderPanel.AddChild(editor);
            armorShaderPanel.AddChild(galleryTab);
            armorShaderPanel.AddChild(controlPanel);

            var settingsPanel = Panel.New().Build();

            _ui = new UserInterface()
                .AddElement(armorShaderPanel)
                .Initialize();
        }

        private void CompileAndReload(string shaderText)
        {
            var isSuccessful = _shaderCompiler!.Compile(shaderText, out var code);
            if (isSuccessful)
                _effect = new Effect(GraphicsDevice, code);
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
                effectParameter.Apply(_effect!);

            var texture = (Texture2D)GraphicsDevice.Textures[1];
            _effect.Parameters[TerrariaShaderParameters.Armor.SecondaryImageSize]
                .SetValue(new Vector2(texture.Width, texture.Height));
            _effect.Parameters[TerrariaShaderParameters.Armor.Direction].SetValue(1f);
            _effect.Parameters[TerrariaShaderParameters.Armor.TargetPosition].SetValue(Vector2.One);

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

        #region UI Methods

        private Panel BuildArmorGalleryPanel(Point position, SpriteFont font)
        {
            const int armorCountHorizontal = 3;
            const int armorCountVertical = 3;

            const int spaceBetweenX = Terraria.PlayerWidth + 20;
            const int spaceBetweenY = Terraria.PlayerHeight + 20;

            var armorGallery = Panel.New().SetBounds(position.X, position.Y, 0, 0).Build();

            var headSkinTexture = _assetManager!.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 0);
            var bodySkinTexture = _assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 3);
            var legSkinTexture = _assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 10);

            for (var i = 0; i < armorCountHorizontal; i++)
            {
                for (var j = 0; j < armorCountVertical; j++)
                {
                    armorGallery.AddChild(
                        PlayerTile.New()
                            .SetIds(_assetManager.ArmorIds[0], _assetManager.ArmorIds[1], _assetManager.ArmorIds[2])
                            .SetSkinTextures(headSkinTexture, bodySkinTexture, legSkinTexture)
                            .SetTextureLoadingCallback(_assetManager.LoadTerrariaTexture)
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

        private Panel BuildSolidGalleryPanel(Point position)
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

        private Panel BuildControlPanel(Point position, SpriteFont font)
        {
            const int labelHeight = 20;

            const int sliderWidth = 100;
            const int sliderHeight = 10;
            const int sliderPadding = 20;

            const int inputFieldWidth = 40;
            const int inputFieldHeight = 20;

            var controlPanel = Panel.New().SetBounds(position.X, position.Y, 0, 0).Build();

            CreateColorSliders((Float3EffectParameterWrapper)_effectParameters![ArmorShaderParam.Color], 0, 0, "Color");
            CreateColorSliders((Float3EffectParameterWrapper)_effectParameters![ArmorShaderParam.SecondaryColor], 180,
                0, "Secondary color");

            CreateLabeledSlider(0, 100, "Time", f => _timeScalar = f, 0, 8f, _timeScalar);
            CreateLabeledSlider(180, 100, "Opacity",
                ((FloatEffectParameterWrapper)_effectParameters![ArmorShaderParam.Opacity]).Set, 0, 1f, 1f);
            CreateLabeledSlider(0, 160, "Saturation",
                ((FloatEffectParameterWrapper)_effectParameters![ArmorShaderParam.Saturation]).Set, 0, 1f, 1f);
            CreateLabeledSlider(180, 160, "Rotation",
                ((FloatEffectParameterWrapper)_effectParameters![ArmorShaderParam.Rotation]).Set, -2 * MathF.PI,
                2 * MathF.PI, 0f);

            CreatePassSlider(0, 220);

            CreateImageControl(360, 0);

            controlPanel.SizeToContents();
            return controlPanel;

            void CreateColorSliders(Float3EffectParameterWrapper parameter, int x, int y, string labelText)
            {
                CreateLabel(labelText, x, y);

                CreateColorSlider(y + sliderPadding * 0, parameter.SetX, Color.Red);
                CreateColorSlider(y + sliderPadding * 1, parameter.SetY, Color.Green);
                CreateColorSlider(y + sliderPadding * 2, parameter.SetZ, Color.Blue);

                void CreateColorSlider(int yOffset, Action<float> callback, Color color)
                {
                    CreateSliderWithInputField(x, y + labelHeight + yOffset, callback, 0f, 2f, 1f, color);
                }
            }

            void CreatePassSlider(int x, int y)
            {
                const int width = 280;

                CreateLabel("Pass", x, y);
                var passLabel = CreateLabel(_effect!.CurrentTechnique.Passes.First().Name, x + width + sliderPadding,
                    y + labelHeight);
                var passSlider = Slider.New()
                    .SetMinMaxValues(0, _effect.CurrentTechnique.Passes.Count - 1)
                    .SetBounds(position.X, position.Y + y + labelHeight + sliderHeight / 2, width, sliderHeight)
                    .Build();
                passSlider.ValueChanged += f => _drawHelper!.SetPassIndex((int)Math.Round(f));
                passSlider.ValueChanged += f =>
                    passLabel.SetText(_effect.CurrentTechnique.Passes[(int)Math.Round(f)].Name);
                controlPanel.AddChild(passSlider);
            }

            void CreateImageControl(int x, int y)
            {
                CreateLabel("Image", x, y);
                var imagePanel = TexturePreview.New()
                    .SetTexture(_noise)
                    .SetFont(font)
                    .SetBounds(position.X + x, position.Y + y + 20, 160, 160)
                    .Build();
                var sizeLabel = CreateLabel($"{_noise.Width}x{_noise.Height}", x, y + 180);
                var imageInputField = TextInputField.New()
                    .SetFont(font)
                    .SetBounds(position.X + x, position.Y + y + 220, 204, inputFieldHeight)
                    .Build();

                var internalImages = AssetManager.GetFiles("Images");
                var entries = new ScrollableListItem<string>[internalImages.Length + 1];
                entries[0] = new ScrollableListItem<string>("None", string.Empty);
                for (var i = 1; i < entries.Length; i++)
                {
                    var fileName = internalImages[i - 1];
                    entries[i] = new ScrollableListItem<string>(fileName, fileName);
                }

                var internalImageScrollList = ScrollableList<string>.New()
                    .SetListItems(entries)
                    .SetItemHeight(20)
                    .SetFont(font)
                    .SetBounds(position.X + x + 180, position.Y + y + 20, 120, 160)
                    .Build();
                internalImageScrollList.ValueChanged += s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    var texture = _assetManager!.LoadImage(s);
                    imagePanel.SetTexture(texture);
                    GraphicsDevice.Textures[1] = texture;
                };

                imageInputField.SetValue(_configManager.Config.TerrariaInstallationPath.Value +
                                         @"\Content\Images\Misc\noise.xnb");
                imageInputField.Commit += s =>
                {
                    if (Path.IsPathFullyQualified(s))
                    {
                        if (_assetManager!.TryLoadExternalImage(s, out var externalImage))
                        {
                            imagePanel.SetTexture(externalImage);
                            GraphicsDevice.Textures[1] = externalImage;
                        }

                        return;
                    }

                    if (_assetManager!.TryLoadImage(s, out var image))
                        imagePanel.SetTexture(image);
                };

                controlPanel.AddChild(imagePanel);
                controlPanel.AddChild(internalImageScrollList);
                controlPanel.AddChild(imageInputField);
            }

            void CreateLabeledSlider(int x, int y, string labelText, Action<float> callback, float minValue,
                float maxValue, float startValue, Color? color = null)
            {
                CreateLabel(labelText, x, y);
                CreateSliderWithInputField(x, y + labelHeight, callback, minValue, maxValue, startValue, color);
            }

            Label CreateLabel(string text, int x, int y)
            {
                var label = Label.New()
                    .SetFont(font)
                    .SetText(text)
                    .SetBounds(position.X + x, position.Y + y, 0, 0)
                    .Build();

                controlPanel.AddChild(
                    label
                );

                return label;
            }

            void CreateSliderWithInputField(int x, int y, Action<float> callback, float minValue, float maxValue,
                float startValue, Color? color = null)
            {
                var slider = Slider.New()
                    .SetMinMaxValues(minValue, maxValue)
                    .SetBackgroundColor(color ?? Color.White)
                    .SetBounds(position.X + x, position.Y + y + sliderHeight / 2, sliderWidth, sliderHeight)
                    .Build();

                var inputField = FloatInputField.New()
                    .SetFont(font)
                    .SetBounds(position.X + x + sliderWidth + sliderPadding, position.Y + y, inputFieldWidth,
                        inputFieldHeight)
                    .Build();

                slider.ValueChanged += callback;
                slider.ValueChanged += inputField.SetValue;

                inputField.Commit += callback;
                inputField.Commit += slider.SetValue;

                slider.SetValue(startValue);

                controlPanel.AddChild(slider);
                controlPanel.AddChild(inputField);
            }
        }

        #endregion
    }
}