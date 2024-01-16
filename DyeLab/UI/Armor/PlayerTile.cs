using DyeLab.Assets;
using DyeLab.Assets.Constants;
using DyeLab.Input.Constants;
using DyeLab.UI.Armor.Constants;
using DyeLab.UI.Constants;
using DyeLab.UI.Interfaces;
using DyeLab.UI.ScrollableList;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.Armor;

public class PlayerTile : UIElement, IClickable
{
    private readonly SkinTextureSet _skinTextures;
    private readonly ArmorSourceRectangles _sourceRectangles;
    private readonly ArmorItem[] _items;
    private int _frame;

    private PlayerTile(ArmorItem[] armorItems, SkinTextureSet skinTextures)
    {
        if (armorItems.Length != 3)
            throw new ArgumentException($"{nameof(armorItems)} must have a length of 3.");

        _items = armorItems;
        _skinTextures = skinTextures;
        _sourceRectangles = new ArmorSourceRectangles();
        _sourceRectangles.Calculate(0);
    }

    public void SetFrame(int frame)
    {
        if (frame == _frame)
            return;

        if (frame is < 0 or >= Terraria.PlayerFrames)
            throw new ArgumentOutOfRangeException(nameof(frame), frame,
                $"Frame must be between 0 and {Terraria.PlayerFrames - 1}");

        _frame = frame;
        _sourceRectangles.Calculate(frame);
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        var bounce = _frame >= 7 && _frame % 7 < 3;
        var bouncePosition = new Vector2(0, bounce ? -2f : 0f);
        DrawIfNotNull(_skinTextures.Head, Vector2.Zero, _sourceRectangles.Legacy, false);
        DrawIfNotNull(_skinTextures.Arms, bouncePosition, _sourceRectangles.BackArm, false);
        DrawIfNotNull(_skinTextures.Body, bouncePosition, _sourceRectangles.Torso, false);
        DrawIfNotNull(_skinTextures.Legs, Vector2.Zero, _sourceRectangles.Legacy, false);
        DrawIfNotNull(_skinTextures.Arms, bouncePosition, _sourceRectangles.FrontArm, false);

        DrawIfNotNull(_items[0].Texture, Vector2.Zero, _sourceRectangles.Legacy, true);
        DrawChestArmorIfNotNull(_items[1].Texture, true);
        DrawIfNotNull(_items[2].Texture, Vector2.Zero, _sourceRectangles.Legacy, true);

        void DrawIfNotNull(Texture2D? texture, Vector2 position, Rectangle sourceRectangle, bool withEffect)
        {
            if (texture == null) return;
            drawHelper.DrawTexture(texture, position, sourceRectangle, Color.White, withEffect);
        }

        void DrawChestArmorIfNotNull(Texture2D? texture, bool withEffect)
        {
            if (texture == null) return;
            
            drawHelper.DrawTexture(texture, bouncePosition, _sourceRectangles.BackArm, Color.White, withEffect);
            drawHelper.DrawTexture(texture, bouncePosition, _sourceRectangles.Torso, Color.White, withEffect);
            if (_sourceRectangles.ShoulderDrawMode == ShoulderDrawMode.Under)
                DrawShoulder();
            drawHelper.DrawTexture(texture, bouncePosition, _sourceRectangles.FrontArm, Color.White, withEffect);
            if (_sourceRectangles.ShoulderDrawMode == ShoulderDrawMode.Over)
                DrawShoulder();

            void DrawShoulder() => drawHelper.DrawTexture(texture, bouncePosition, _sourceRectangles.Shoulder, Color.White, withEffect);
        }
    }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (!buttons.HasFlag(MouseButtons.LMB))
            return;
        
        const float headRatio = 0.5f;
        const float bodyRatio = 0.75f;
        
        var index = ((float)mousePosition.Y / Height) switch
        {
            < headRatio => 0,
            < bodyRatio => 1,
            _ => 2
        };
        for (var i = 0; i < 3; i++)
        {
            if (i == index)
            {
                _items[i].ToggleList();
                continue;
            }

            _items[i].HideList();
        }
    }

    public void OnLoseFocus()
    {
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<PlayerTile>
    {
        private ExternalTextureKey[]? _headKeys;
        private ExternalTextureKey[]? _bodyKeys;
        private ExternalTextureKey[]? _legKeys;

        private SkinTextureSet? _skinTextures;

        private SpriteFont? _font;

        private Func<TerrariaTextureType, int, Texture2D>? _loadTextureDelegate;

        public Builder()
        {
            AddValidationStep(() => _headKeys != null, "Head texture keys have not been set.");
            AddValidationStep(() => _bodyKeys != null, "Body texture keys have not been set.");
            AddValidationStep(() => _legKeys != null, "Leg texture keys have not been set.");
            AddValidationStep(() => _font != null, "Font has not been set.");
            AddValidationStep(() => _loadTextureDelegate != null, "Texture loading delegate has not been set.");
        }

        public Builder SetIds(ExternalTextureKey[] headKeys, ExternalTextureKey[] bodyKeys, ExternalTextureKey[] legKeys)
        {
            _headKeys = headKeys;
            _bodyKeys = bodyKeys;
            _legKeys = legKeys;
            return this;
        }

        public Builder SetSkinTextures(SkinTextureSet skinTextures)
        {
            _skinTextures = skinTextures;
            return this;
        }

        public Builder SetTextureLoadingDelegate(Func<TerrariaTextureType, int, Texture2D> func)
        {
            _loadTextureDelegate = func;
            return this;
        }

        public Builder SetFont(SpriteFont font)
        {
            _font = font;
            return this;
        }

        protected override PlayerTile BuildElement()
        {
            const int listOffsetX = 50;
            const int listItemHeight = 20;
            const int listWidth = 200;
            const int listHeight = 200;
            
            var headList = CreateList(_headKeys!, 0);
            var bodyList = CreateList(_bodyKeys!, 1);
            var legList = CreateList(_legKeys!, 2);

            var headItem = new ArmorItem(headList, i => _loadTextureDelegate!(TerrariaTextureType.ArmorHead, i));
            var bodyItem = new ArmorItem(bodyList, i => _loadTextureDelegate!(TerrariaTextureType.ArmorBody, i));
            var legItem = new ArmorItem(legList, i => _loadTextureDelegate!(TerrariaTextureType.ArmorLeg, i));

            var playerTile = new PlayerTile(new []{headItem, bodyItem, legItem}, _skinTextures ?? new SkinTextureSet());
            playerTile.AddChild(headList);
            playerTile.AddChild(bodyList);
            playerTile.AddChild(legList);
            return playerTile;

            ScrollableList<int> CreateList(IEnumerable<ExternalTextureKey> data, int index)
            {
                var list = ScrollableList<int>.New()
                    .SetListItems(data.Select((x, i) => new ScrollableListItem<int>(x.TextureName, x.ExternalId)).ToArray())
                    .SetItemHeight(listItemHeight)
                    .SetFont(_font!)
                    .MarkAsPopup()
                    .SetBounds(X + listOffsetX, Y + index * (Terraria.PlayerHeight / 3), listWidth, listHeight)
                    .SetDrawLayer(DrawLayer.Foreground)
                    .Build();
                list.SetActive(false);
                return list;
            }
        }
    }
}