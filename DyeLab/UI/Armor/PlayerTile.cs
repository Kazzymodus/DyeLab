using System.Runtime.CompilerServices;
using DyeLab.Assets.Constants;
using DyeLab.Input.Constants;
using DyeLab.UI.Constants;
using DyeLab.UI.Interfaces;
using DyeLab.UI.ScrollableList;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.Armor;

public class PlayerTile : UIElement, IClickable
{
    private readonly Texture2D?[] _playerSkin;
    private readonly ArmorItem[] _items;

    private PlayerTile(ArmorItem[] armorItems, Texture2D?[] skins)
    {
        ThrowIfLengthNotThree(armorItems);
        ThrowIfLengthNotThree(skins);

        _items = armorItems;
        _playerSkin = skins;

        void ThrowIfLengthNotThree(Array property, [CallerArgumentExpression("property")] string? propertyName = null)
        {
            if (property.Length != 3)
                throw new ArgumentException($"{propertyName} must have a length of 3.");
        }
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        DrawIfNotNull(_playerSkin[0], false);
        DrawChestArmorIfNotNull(_playerSkin[1], false);
        DrawIfNotNull(_playerSkin[2], false);
        
        DrawIfNotNull(_items[0].Texture, true);
        DrawChestArmorIfNotNull(_items[1].Texture, true);
        DrawIfNotNull(_items[2].Texture, true);

        void DrawIfNotNull(Texture2D? texture, bool withEffect)
        {
            if (texture == null) return;
            drawHelper.DrawTexture(texture, Vector2.Zero, Terraria.PlayerSourceRectangle, Color.White, withEffect);
        }

        void DrawChestArmorIfNotNull(Texture2D? texture, bool withEffect)
        {
            if (texture == null) return;
            drawHelper.DrawTexture(texture, Vector2.Zero, Terraria.BodyBackArmSourceRectangle, Color.White, withEffect);
            drawHelper.DrawTexture(texture, Vector2.Zero, Terraria.PlayerSourceRectangle, Color.White, withEffect);
            drawHelper.DrawTexture(texture, Vector2.Zero, Terraria.BodyFrontArmSourceRectangle, Color.White, withEffect);
            drawHelper.DrawTexture(texture, Vector2.Zero, Terraria.BodySpaulderSourceRectangle, Color.White, withEffect);
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
        private string[]? _headIds;
        private string[]? _bodyIds;
        private string[]? _legIds;

        private readonly Texture2D?[] _skinTextures = new Texture2D?[3];

        private SpriteFont? _font;

        private Func<TerrariaTextureType, int, Texture2D>? _loadTextureDelegate;

        public Builder()
        {
            AddValidationStep(() => _headIds != null, "Head IDs have not been set.");
            AddValidationStep(() => _bodyIds != null, "Body IDs have not been set.");
            AddValidationStep(() => _legIds != null, "Leg IDs have not been set.");
            AddValidationStep(() => _font != null, "Font has not been set.");
            AddValidationStep(() => _loadTextureDelegate != null, "Texture loading delegate has not been set.");
        }

        public Builder SetIds(string[] headIds, string[] bodyIds, string[] legIds)
        {
            _headIds = headIds;
            _bodyIds = bodyIds;
            _legIds = legIds;
            return this;
        }

        public Builder SetSkinTextures(Texture2D? head, Texture2D? body, Texture2D? legs)
        {
            _skinTextures[0] = head;
            _skinTextures[1] = body;
            _skinTextures[2] = legs;
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
            
            var headList = CreateList(_headIds!, 0);
            var bodyList = CreateList(_bodyIds!, 1);
            var legList = CreateList(_legIds!, 2);

            var headItem = new ArmorItem(headList, i => _loadTextureDelegate!(TerrariaTextureType.ArmorHead, i));
            var bodyItem = new ArmorItem(bodyList, i => _loadTextureDelegate!(TerrariaTextureType.ArmorBody, i));
            var legItem = new ArmorItem(legList, i => _loadTextureDelegate!(TerrariaTextureType.ArmorLeg, i));

            var playerTile = new PlayerTile(new []{headItem, bodyItem, legItem}, _skinTextures);
            playerTile.AddChild(headList);
            playerTile.AddChild(bodyList);
            playerTile.AddChild(legList);
            return playerTile;

            ScrollableList<int> CreateList(IEnumerable<string> data, int index)
            {
                var list = ScrollableList<int>.New()
                    .SetListItems(data.Select((x, i) => new ScrollableListItem<int>(x, i)).ToArray())
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