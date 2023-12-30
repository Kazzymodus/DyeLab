using DyeLab.UI.Constants;
using DyeLab.UI.Exceptions;

namespace DyeLab.UI;

public abstract class UIElementBuilder<T> where T : UIElement
{
    protected int X { get; private set; }
    protected int Y { get; private set; }
    private int _width;
    private int _height;

    private DrawLayer _drawLayer;
    
    private ICollection<UIElement> _children = new List<UIElement>();

    private ICollection<(Func<bool> func, string failureMessage)> _validationSteps =
        new List<(Func<bool> func, string failureMessage)>();

    protected void AddValidationStep(Func<bool> validationFunc, string failureMessage)
    {
        _validationSteps.Add((validationFunc, failureMessage));
    }

    public UIElementBuilder<T> AddChild(UIElement element)
    {
        _children.Add(element);
        return this;
    }

    public UIElementBuilder<T> SetBounds(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        _width = width;
        _height = height;
        return this;
    }

    public UIElementBuilder<T> SetDrawLayer(DrawLayer drawLayer)
    {
        _drawLayer = drawLayer;
        return this;
    }

    public T Build()
    {
        foreach (var (validationFunc, failureMessage) in _validationSteps)
        {
            if (!validationFunc())
                throw new InvalidUIElementException(failureMessage);
        }
        
        var element = BuildElement();
        element.SetBounds(X, Y, _width, _height);
        element.SetDrawLayer(_drawLayer);
        foreach (var child in _children)
            element.AddChild(child);
        return element;
    }

    protected abstract T BuildElement();
}