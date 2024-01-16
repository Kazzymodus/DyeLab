using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using DyeLab.UI.Constants;
using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public abstract class UIElement
{
    protected int X { get; private set; }
    protected int Y { get; private set; }
    protected int Width { get; private set; }
    protected int Height { get; private set; }

    private DrawLayer _drawLayer = DrawLayer.Default;

    public Point Position => new(X, Y);
    public Rectangle Bounds => new(X, Y, Width, Height);

    private readonly IList<UIElement> _children = new List<UIElement>();

    public bool IsActive { get; private set; } = true;

    public static event EventHandler? ActiveChanged;

    public virtual void SetBounds(int x, int y, int width, int height)
    {
        ThrowIfNegative(x);
        ThrowIfNegative(y);
        ThrowIfNegative(width);
        ThrowIfNegative(height);

        X = x;
        Y = y;
        Width = width;
        Height = height;

        void ThrowIfNegative(int property, [CallerArgumentExpression("property")] string? propertyName = null)
        {
            if (property < 0)
                throw new ArgumentOutOfRangeException(propertyName, property,
                    $"{propertyName} must not be negative.");
        }
    }

    public void SizeToContents()
    {
        var width = 0;
        var height = 0;

        CheckChildren(this);

        void CheckChildren(UIElement element)
        {
            foreach (var child in element._children)
            {
                width = Math.Max(width, child.X + child.Width - X);
                height = Math.Max(height, child.Y + child.Height - Y);

                CheckChildren(child);
            }
        }

        SetBounds(X, Y, width, height);
    }

    public void AddChild(UIElement child)
    {
        if (_children.Contains(child))
            throw new ArgumentException($"{child.GetType().Name} is already a child of {GetType().Name}.");
        _children.Add(child);
    }

    public void SetDrawLayer(DrawLayer drawLayer)
    {
        _drawLayer = drawLayer;
    }

    public void SetActive(bool value)
    {
        if (IsActive == value)
            return;

        IsActive = value;
        ActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
        ActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    public void GetElementsOfType<T>(in List<T>[] elements) where T : class
    {
        if (GetType().IsAssignableTo(typeof(T)))
            elements[(int)_drawLayer].Add((this as T)!);

        foreach (var child in _children)
            child.GetElementsOfType(elements);
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        UpdateElement(gameTime);

        foreach (var child in _children)
            child.Update(gameTime);
    }

    protected virtual void UpdateElement(GameTime gameTime)
    {
    }

    public void QueueDraw(in List<UIElement>[] queue)
    {
        if (!IsActive)
            return;

        queue[(int)_drawLayer].Add(this);

        foreach (var child in _children)
            child.QueueDraw(queue);
    }

    public void Draw(DrawHelper drawHelper)
    {
        drawHelper.DrawOffset = new Vector2(Position.X, Position.Y);
        // if (Width > 0 && Height > 0)
        //     drawHelper.DrawSolid(Vector2.Zero, Width, Height, Color.FromNonPremultiplied(255, 0, 0, 40));
        DrawElement(drawHelper);
    }

    protected virtual void DrawElement(DrawHelper drawHelper)
    {
    }
}