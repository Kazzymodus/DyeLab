using DyeLab.Input.Constants;
using DyeLab.UI.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DyeLab.UI;

public class UserInterface
{
    private bool _isInitialized;

    private readonly ICollection<UIElement> _elements = new List<UIElement>();
    private List<UIElement>[] _drawQueues = new List<UIElement>[(int)DrawLayer.Count];

    private List<IClickable>[]? _orderedClickables;
    private List<IScrollable>[]? _orderedScrollables;
    private IClickable? _currentFocus;

    private bool _shouldResortDraws;

    public UserInterface AddElement(UIElement uiElement)
    {
        if (_isInitialized)
            throw new InvalidOperationException(
                "Can not add elements once " + typeof(UserInterface) + " is initialized");

        _elements.Add(uiElement);

        return this;
    }

    public UserInterface Initialize()
    {
        if (_isInitialized)
            return this;

        _orderedClickables = GetElementsOfTypeOrdered<IClickable>();
        _orderedScrollables = GetElementsOfTypeOrdered<IScrollable>();
        Mouse.ClickedEXT += OnClick;
        Mouse.ScrolledEXT += OnScroll;
        
        SortDraws();
        
        UIElement.ActiveChanged += () => _shouldResortDraws = true;

        _isInitialized = true;
        return this;
    }

    private void SortDraws()
    {
        for (var i = 0; i < _drawQueues.Length; i++)
            _drawQueues[i] = new List<UIElement>();

        foreach (var element in _elements)
            element.QueueDraw(_drawQueues);
    }

    public void Update(GameTime gameTime)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("UI is not initialized.");

        foreach (var element in _elements)
            element.Update(gameTime);
    }

    public void Draw(DrawHelper drawHelper)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("UI is not initialized.");

        if (_shouldResortDraws)
        {
            SortDraws();
            _shouldResortDraws = false;
        }

        foreach (var queue in _drawQueues)
        {
            foreach (var element in queue)
            {
                element.Draw(drawHelper);
            }
        }
    }

    private List<T>[] GetElementsOfTypeOrdered<T>() where T : class
    {
        const int layerCount = (int)DrawLayer.Count;
        var elementLayers = new List<T>[layerCount];
        for (var i = 0; i < layerCount; i++)
            elementLayers[i] = new List<T>();

        foreach (var element in _elements)
            element.GetElementsOfType(elementLayers);

        return elementLayers;
    }

    private void OnClick(int button)
    {
        var mousePosition = GetMousePosition();

        var previousFocus = _currentFocus;
        if (previousFocus != null)
        {
            if (previousFocus.Bounds.Contains(mousePosition))
            {
                previousFocus.OnClick((MouseButton)button, mousePosition - previousFocus.Bounds.Location);
                return;
            }

            previousFocus.OnLoseFocus();
            _currentFocus = null;
        }

        for (var i = _orderedClickables!.Length - 1; i >= 0; i--)
        {
            foreach (var clickable in _orderedClickables[i])
            {
                if (clickable == previousFocus || !clickable.Bounds.Contains(mousePosition) || !clickable.IsActive)
                    continue;

                _currentFocus = clickable;
                clickable.OnFocus();
                clickable.OnClick((MouseButton)button, mousePosition - clickable.Bounds.Location);
                return;
            }
        }
    }

    private void OnScroll(int amount)
    {
        if (amount == 0)
            return;
        
        var mousePosition = GetMousePosition();

        for (var i = _orderedScrollables!.Length - 1; i >= 0; i--)
        {
            foreach (var scrollable in _orderedScrollables[i])
            {
                if (!scrollable.Bounds.Contains(mousePosition) || !scrollable.IsActive)
                    continue;

                scrollable.OnScroll(amount);
                return;
            }
        }
    }

    private static Point GetMousePosition()
    {
        var mouseState = Mouse.GetState();
        return new Point(mouseState.X, mouseState.Y);
    }
}