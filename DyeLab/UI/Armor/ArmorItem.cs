using DyeLab.UI.ScrollableList;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.Armor;

public class ArmorItem
{
    public ArmorItem(ScrollableList<int> selectionList, Func<int, Texture2D> loadTextureCallback)
    {
        _selectionList = selectionList;
        _selectionList.ValueChanged += i =>
        {
            Texture = i == 0 ? null : loadTextureCallback(i);
        };
    }

    public Texture2D? Texture { get; private set; }

    private readonly ScrollableList<int> _selectionList;

    public void ToggleList()
    {
        _selectionList.ToggleActive();
    }

    public void HideList()
    {
        _selectionList.SetActive(false);
    }
}