using DyeLab.Input.Constants;
using DyeLab.UI.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI;

public class TabBar : UIElement, IClickable
{
    private readonly Tab[] _tabs;
    private int _activeTab;

    private readonly SpriteFont _font;

    private TabBar(Tab[] tabs, SpriteFont font)
    {
        _tabs = tabs;
        _font = font;

        for (var i = 1; i < tabs.Length; i++)
        {
            tabs[i].SetPanelActive(false);
        }
    }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (!buttons.HasFlag(MouseButtons.LMB))
            return;

        var tabIndex = (int)(mousePosition.X / (float)Width * _tabs.Length);
        var newTab = Math.Clamp(tabIndex, 0, _tabs.Length - 1);

        if (newTab == _activeTab)
            return;

        _tabs[newTab].SetPanelActive(true);
        _tabs[_activeTab].SetPanelActive(false);
        _activeTab = newTab;
    }

    public void OnLoseFocus()
    {
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        var tabWidth = Width / _tabs.Length;

        for (var i = 0; i < _tabs.Length; i++)
        {
            var tab = _tabs[i];
            var drawPosition = new Vector2(tabWidth * i, 0);
            drawHelper.DrawSolid(drawPosition, tabWidth, Height, Color.White);
            drawHelper.DrawText(_font, tab.Label, drawPosition, i == _activeTab ? Color.Black : Color.DarkGray);
        }
    }

    private class Tab
    {
        public Tab(string label, UIElement uiElement)
        {
            Label = label;
            SetPanelActive = uiElement.SetActive;
        }

        public string Label { get; }
        public Action<bool> SetPanelActive { get; }
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<TabBar>
    {
        private readonly ICollection<Tab> _tabs = new List<Tab>();

        private SpriteFont? _font;

        public Builder()
        {
            AddValidationStep(() => _tabs.Count > 0, "TabBar must contain at least one tab.");
            AddValidationStep(() => _font != null, "Font has not been set.");
        }

        public Builder AddTab(string label, UIElement panel)
        {
            _tabs.Add(new Tab(label, panel));
            AddChild(panel);
            return this;
        }

        public Builder SetFont(SpriteFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            return this;
        }

        protected override TabBar BuildElement()
        {
            return new TabBar(_tabs.ToArray(), _font!);
        }
    }
}