using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DyeLab.UI.InputField;

public class Cursor
{
    private bool _isEnabled;
    public int Position { get; private set; }
    
    private bool _hasCursorMoved;
    private TimeSpan _cursorLastMoved;

    private readonly float _cursorBlinkDelay;
    private readonly float _cursorBlinkDuration;

    public bool IsVisible { get; private set; }

    public Cursor(float cursorBlinkDelay, float cursorBlinkDuration)
    {
        _cursorBlinkDelay = cursorBlinkDelay;
        _cursorBlinkDuration = cursorBlinkDuration;
        _cursorLastMoved = TimeSpan.Zero;
    }

    public void Enable()
    {
        if (_isEnabled)
            return;

        _isEnabled = true;
    }

    public void Disable()
    {
        if (!_isEnabled)
            return;

        _isEnabled = false;
        IsVisible = false;
    }
    
    public void Update(GameTime gameTime, KeyboardState keyboardState, out bool cursorMoved)
    {
        cursorMoved = false;
        
        if (!_isEnabled)
            return;

        if (_hasCursorMoved)
        {
            _hasCursorMoved = false;
            _cursorLastMoved = gameTime.TotalGameTime;
            cursorMoved = true;
        }

        var timeSinceMoved = (gameTime.TotalGameTime - _cursorLastMoved).TotalSeconds;
        IsVisible = timeSinceMoved < _cursorBlinkDelay || timeSinceMoved % (_cursorBlinkDuration * 2) <= _cursorBlinkDuration;
    }
    
    public void MoveCursorBy(int amount)
    {
        Position += amount;
        if (Position < 0)
            Position = 0;
        _hasCursorMoved = true;
    }

    public void MoveCursorTo(int position)
    {
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(position), position, "Position must not be negative.");

        Position = position;
        _hasCursorMoved = true;
    }
}