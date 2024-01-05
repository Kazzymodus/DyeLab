using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.ScrollableList;

public class ScrollableList<T> : UIElement, IClickable, IScrollable
{
    private IList<ScrollableListItem<T>> _items;
    private readonly SpriteFont _font;
    private readonly int _itemHeight;

    private int _selectedIndex;
    private int _scroll;

    private readonly bool _isPopup;

    public event Action<T>? ValueChanged;

    private ScrollableList(IEnumerable<ScrollableListItem<T>> entries, SpriteFont font, int itemHeight, bool isPopup)
    {
        _items = entries.ToList();
        _font = font;
        _itemHeight = itemHeight;
        _isPopup = isPopup;
    }

    public void SetEntries(IEnumerable<ScrollableListItem<T>> entries)
    {
        var selectedItem = _items[_selectedIndex];
        _items = entries.ToList();

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if (item.Label != selectedItem.Label || !Equals(item.Value, selectedItem.Value)) continue;

            _selectedIndex = i;
            return;
        }

        _selectedIndex = 0;
        ValueChanged?.Invoke(_items[_selectedIndex].Value);
    }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (!buttons.HasFlag(MouseButtons.LMB))
            return;

        var index = _scroll + mousePosition.Y / _itemHeight;
        if (index >= _items.Count)
            return;

        _selectedIndex = index;

        ValueChanged?.Invoke(_items[_selectedIndex].Value);
    }

    public void OnLoseFocus()
    {
        if (_isPopup)
            SetActive(false);
    }

    public void OnScroll(int amount)
    {
        _scroll = Math.Clamp(_scroll - amount, 0, _items.Count - Height / _itemHeight);
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawSolid(new Vector2(0, 0), Width, Height, new Color(40, 40, 40, 255));
        var endIndex = _scroll + Height / _itemHeight - 1;
        if (endIndex >= _items.Count)
            endIndex = _items.Count - 1;

        var j = 0;
        for (var i = _scroll; i <= endIndex; i++, j++)
        {
            var yPosition = j * _itemHeight;
            var color =
                i == _selectedIndex
                    ? new Color(160, 160, 160, 255)
                    : i % 2 == 0
                        ? new Color(40, 40, 40, 255)
                        : new Color(60, 60, 60, 255);
            drawHelper.DrawSolid(new Vector2(0, yPosition), Width, _itemHeight, color);

            var drawText = _items[i].Label;
            var textSize = _font.MeasureString(drawText);
            if (textSize.X > Width)
                drawText = drawText[..(int)(Width / (textSize.X / drawText.Length))];
            
            drawHelper.DrawText(_font, drawText, new Vector2(0, yPosition), Color.White);
        }
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<ScrollableList<T>>
    {
        private IEnumerable<ScrollableListItem<T>>? _items;
        private SpriteFont? _font;
        private int _itemHeight;
        private bool _isPopup;

        public Builder()
        {
            AddValidationStep(() => _items != null && _items.Any(), "No items have been set.");
            AddValidationStep(() => _font != null, "Font has not been set.");
            AddValidationStep(() => _itemHeight > 0, "Item height is zero.");
        }

        public Builder SetListItems(IEnumerable<ScrollableListItem<T>> entries)
        {
            _items = entries ?? throw new ArgumentNullException(nameof(entries));
            return this;
        }

        public Builder SetFont(SpriteFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            return this;
        }

        public Builder SetItemHeight(int height)
        {
            _itemHeight = height;
            return this;
        }

        public Builder MarkAsPopup()
        {
            _isPopup = true;
            return this;
        }

        protected override ScrollableList<T> BuildElement()
        {
            return new ScrollableList<T>(_items!, _font!, _itemHeight, _isPopup);
        }
    }
}