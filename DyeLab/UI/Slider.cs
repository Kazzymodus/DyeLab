using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DyeLab.UI;

public class Slider : UIElement, IClickable, IScrollable
{
    private float _sliderValue;
    private float _minValue;
    private float _maxValue;
    private readonly Color _backgroundColor;

    private bool _isDragging;

    public event Action<float>? ValueChanged;
    
    private Slider(float minValue, float maxValue, Color? backgroundColor = null)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _backgroundColor = backgroundColor ?? Color.White;
    }

    public float GetValue() => _minValue + _sliderValue * (_maxValue - _minValue);

    public void SetValue(float value)
    {
        var clampedValue = Math.Clamp(value, _minValue, _maxValue);
        var newValue = (clampedValue - _minValue) / (_maxValue - _minValue);
        if (float.IsNaN(newValue))
            newValue = 0;

        if (_sliderValue.Equals(newValue) && value.Equals(clampedValue))
            return;
            
        _sliderValue = newValue;
        ValueChanged?.Invoke(clampedValue);
    }

    public void SetMinMaxValue(float minValue, float maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentException($"{nameof(minValue)} must be smaller than {nameof(maxValue)}");

        _minValue = minValue;
        _maxValue = maxValue;

        var currentValue = GetValue();
        var clampedValue = Math.Clamp(currentValue, minValue, maxValue);
        if (!currentValue.Equals(clampedValue))
            SetValue(clampedValue);
    }

    protected override void UpdateElement(GameTime gameTime)
    {
        if (!_isDragging)
            return;

        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton != ButtonState.Pressed)
        {
            _isDragging = false;
            return;
        }

        _sliderValue = Math.Clamp((mouseState.X - X) / (float)Width, 0f, 1f);
        ValueChanged?.Invoke(GetValue());
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        var barHeight = (int)(Height * 0.4f);
        var buttonWidth = (int)(Width * 0.05f);
        drawHelper.DrawSolid(new Vector2(buttonWidth * 0.5f, Height * 0.5f - barHeight * 0.5f), Width - buttonWidth, barHeight, _backgroundColor);
        drawHelper.DrawSolid(new Vector2(_sliderValue * (Width - buttonWidth), 0), buttonWidth, Height,
            Color.Black);
    }

    public void OnFocus()
    {
    }

    public void OnClick(MouseButtons buttons, Point mousePosition)
    {
        if (buttons.HasFlag(MouseButtons.LMB))
            _isDragging = true;
    }

    public void OnLoseFocus()
    {
    }

    public void OnScroll(int amount)
    {
        var currentValue = GetValue();
        SetValue(currentValue - amount);
    }

    public static Builder New() => new();

    public class Builder : UIElementBuilder<Slider>
    {
        private float? _minValue;
        private float? _maxValue;
        private Color? _backgroundColor;

        public Builder()
        {
            AddValidationStep(() => _minValue.HasValue && _maxValue.HasValue, "Min and max values have not been set.");
        }

        public Builder SetMinMaxValues(float min, float max)
        {
            if (min > max)
                throw new ArgumentException($"{nameof(min)} must be smaller than {nameof(max)}");

            _minValue = min;
            _maxValue = max;
            return this;
        }

        public Builder SetBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        protected override Slider BuildElement()
        {
            return new Slider(_minValue!.Value, _maxValue!.Value, _backgroundColor);
        }
    }
}