using DyeLab.Assets;
using DyeLab.Assets.Constants;
using DyeLab.UI;
using DyeLab.UI.Armor;
using DyeLab.UI.InputField;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Segments;

public class PlayerFrameGallery : Segment
{
    private readonly SpriteFont _font;
    private readonly AssetManager _assetManager;

    private bool _isWalking;
    private int _frame;
    private int _frameTime;
    private event Action<int>? FrameChanged;

    public PlayerFrameGallery(SpriteFont font, AssetManager assetManager)
    {
        _font = font;
        _assetManager = assetManager;
    }

    public override Panel BuildUI(Point position)
    {
        const int armorCountHorizontal = 3;
        const int armorCountVertical = 3;

        const int spaceBetweenX = Terraria.PlayerWidth + 20;
        const int spaceBetweenY = Terraria.PlayerHeight + 20;

        var panel = Panel.New()
            .SetBounds(position.X, position.Y, 0, 0)
            .Build();

        var armorPanel = Panel.New()
            .SetBounds(position.X, position.Y, 0, 0)
            .Build();
        var solidPanel = Panel.New()
            .SetBounds(position.X, position.Y, 0, 0)
            .Build();

        var skinTextureSet = new SkinTextureSet
        {
            Head = _assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 0),
            Body = _assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 3),
            Arms = _assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 7),
            Legs = _assetManager.LoadTerrariaTexture(TerrariaTextureType.PlayerBase, 10),
        };

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

        for (var i = 0; i < armorCountHorizontal; i++)
        {
            for (var j = 0; j < armorCountVertical; j++)
            {
                var xPosition = position.X + 40 + j * spaceBetweenX;
                var yPosition = position.Y + i * spaceBetweenY;

                var playerTile = PlayerTile.New()
                    .SetIds(_assetManager.ArmorTextures[0], _assetManager.ArmorTextures[1], _assetManager.ArmorTextures[2])
                    .SetSkinTextures(skinTextureSet)
                    .SetTextureLoadingDelegate(_assetManager.LoadTerrariaTexture)
                    .SetFont(_font)
                    .SetBounds(xPosition, yPosition, Terraria.PlayerWidth, Terraria.PlayerHeight)
                    .Build();
                FrameChanged += playerTile.SetFrame;
                armorPanel.AddChild(playerTile);

                var solidTile = SolidFrame.New()
                    .SetSolidSize(Terraria.PlayerWidth, Terraria.PlayerSheetHeight)
                    .SetColor(colours[i * armorCountHorizontal + j])
                    .DrawWithEffect()
                    .SetBounds(xPosition, yPosition, Terraria.PlayerWidth, Terraria.PlayerHeight)
                    .Build();
                solidTile.SetSourceRectanglePosition(0, 0);
                FrameChanged += frame => solidTile.SetSourceRectanglePosition(0, frame * Terraria.PlayerHeight);
                solidPanel.AddChild(solidTile);
            }
        }

        armorPanel.SizeToContents();
        solidPanel.SizeToContents();

        var tabBar = TabBar.New()
            .AddTab("Armors", armorPanel)
            .AddTab("Solids", solidPanel)
            .SetFont(_font)
            .SetBounds(position.X + 20, position.Y + 240, 200, 20)
            .Build();
        panel.AddChild(tabBar);

        var walkLabel = Label.New()
            .SetFont(_font)
            .SetText("Frame")
            .SetBounds(position.X + 30, position.Y + 275, 80, 20)
            .Build();
        panel.AddChild(walkLabel);
        
        var checkbox = Checkbox.New().SetBounds(position.X + 10, position.Y + 300 - 2, 14, 14).Build();
        checkbox.ValueChanged += SetWalkCycle;
        panel.AddChild(checkbox);

        var slider = Slider.New()
            .SetMinMaxValues(0, Terraria.PlayerFrames - 1)
            .SetBounds(position.X + 30, position.Y + 300, 180, 10)
            .Build();
        slider.ValueChanged += (_, args) => SetFrame((int)Math.Round(args.NewValue));
        FrameChanged += i => slider.SetValue(i);
        
        var inputField = IntegerInputField.New()
            .SetFont(_font)
            .SetBounds(position.X + 215, position.Y + 300 - 5, 20, 20)
            .Build();
        inputField.SetValue(0);
        FrameChanged += inputField.SetValue;

        slider.ValueChanged += (_, args) => inputField.SetValue((int)Math.Round(args.NewValue));
        inputField.Commit += (_, args) => slider.SetValue(args.NewValue);
        
        panel.AddChild(slider);
        panel.AddChild(inputField);

        panel.SizeToContents();

        return panel;
    }

    public override void Update(GameTime gameTime)
    {
        if (!_isWalking) return;

        const int frameDelay = 3;

        if (++_frameTime < frameDelay)
            return;

        _frameTime = 0;
        if (++_frame >= Terraria.WalkFrameEnd)
            _frame = Terraria.WalkFrameStart;

        FrameChanged?.Invoke(_frame);
    }

    private void SetWalkCycle(bool value)
    {
        if (_isWalking == value)
            return;

        _isWalking = value;

        if (value)
            SetFrame(Terraria.WalkFrameStart);
    }

    private void SetFrame(int frame)
    {
        if (frame == _frame)
            return;

        if (frame is < 0 or >= Terraria.PlayerFrames)
            throw new ArgumentOutOfRangeException(nameof(frame), frame,
                $"Frame must be between 0 and {Terraria.PlayerFrames - 1}");

        _frame = frame;
        FrameChanged?.Invoke(frame);
    }
}