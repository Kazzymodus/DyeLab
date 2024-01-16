using DyeLab.UI.ScrollableList;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI.Armor;

public class ArmorItem
{
    public ArmorItem(ScrollableList<int> selectionList, Func<int, Texture2D> loadTextureDelegate)
    {
        _selectionList = selectionList;
        _selectionList.ValueChanged += (_, args) =>
        {
            Texture = args.NewValue == 0 ? null : loadTextureDelegate(args.NewValue);
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