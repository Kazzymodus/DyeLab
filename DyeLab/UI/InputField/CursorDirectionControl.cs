using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DyeLab.UI.InputField;

public class CursorDirectionControl
{
    private readonly Keys _key;
    private TimeSpan? _pressStart;
    private readonly Action _cursorAction;
    private const float HoldDelay = 0.75f;

    public CursorDirectionControl(Keys key, Action cursorAction)
    {
        _key = key;
        _cursorAction = cursorAction;
    }

    public void Update(KeyboardState keyboardState, GameTime gameTime)
    {
        if (keyboardState.IsKeyDown(_key))
        {
            if (!_pressStart.HasValue)
            {
                _cursorAction();
                _pressStart = gameTime.TotalGameTime;
                return;
            }

            if ((gameTime.TotalGameTime - _pressStart.Value).TotalSeconds > HoldDelay)
            {
                _cursorAction();
            }

            return;
        }

        if (!_pressStart.HasValue)
            return;

        _pressStart = null;
    }
}