using System.Text;
using DyeLab.Input.Constants;
using DyeLab.UI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DyeLab.UI.InputField;

public abstract class InputField : UIElement
{
    private bool _takingInput;
    public bool IsReadOnly { private get; set; }

    protected StringBuilder Content { get; } = new();
    private string _cachedText = string.Empty;

    private readonly Cursor _cursor;
    protected int CursorPosition => _cursor.Position;
    private TextCoordinates _lastCursorCoordinates;
    private TextCoordinates _scrollOffset;

    private const float HoldDelay = 0.75f;
    private readonly ICollection<CursorDirectionControl> _cursorDirectionControl = new List<CursorDirectionControl>();

    private readonly SpriteFont _font;

    // private Selection _selection;

    private readonly AutoCommitter? _autoCommitter;

    protected InputField(SpriteFont font, float? autoCommitDelay = null)
    {
        _font = font;
        if (autoCommitDelay.HasValue)
            _autoCommitter = new AutoCommitter(autoCommitDelay.Value);
        _cursor = new Cursor(1f, 1f);

        _cursorDirectionControl.Add(new CursorDirectionControl(Keys.Up, () => MoveCursorVertically(-1)));
        _cursorDirectionControl.Add(new CursorDirectionControl(Keys.Down, () => MoveCursorVertically(1)));
        _cursorDirectionControl.Add(new CursorDirectionControl(Keys.Left, () => MoveCursorHorizontally(-1)));
        _cursorDirectionControl.Add(new CursorDirectionControl(Keys.Right, () => MoveCursorHorizontally(1)));
    }

    public void OnFocus()
    {
        if (IsReadOnly)
            return;

        StartTextInput();
    }

    public void OnClick(MouseButton button, Point mousePosition)
    {
        if (IsReadOnly)
            return;

        if (!_takingInput)
        {
            StartTextInput();
        }

        var characterSize = _font.MeasureString("*");
        var clickCoordinates = new TextCoordinates((int)(mousePosition.Y / characterSize.Y) + _scrollOffset.Line,
            (int)Math.Round(mousePosition.X / characterSize.X) + _scrollOffset.Column);
        var clickCursor = TextCoordinatesToPosition(clickCoordinates);
        _cursor.MoveCursorTo(clickCursor);
    }

    private void ParseInput(char input)
    {
        if (IsReadOnly || !_takingInput)
            return;

        const char backspace = '\b';
        const char delete = '\u007f';
        const char enter = '\r';
        const char tab = '\t';

        switch (input)
        {
            case backspace:
            {
                RemoveBehindCursor();
                return;
            }
            case delete:
                RemoveAtCursor();
                return;
            case enter:
            {
                HandleReturn();
                return;
            }
            case tab:
            {
                HandleTab();
                return;
            }
        }

        if (!IsValidCharacter(input))
            return;

        InsertAtCursor(input);
    }

    protected void CacheDrawText()
    {
        _cachedText = Content.ToString();
    }

    protected void InsertAtCursor(char c)
    {
        Content.Insert(_cursor.Position, c);
        _cursor.MoveCursorBy(1);
        OnContentChange();
    }

    protected void InsertAtCursor(string s)
    {
        Content.Insert(_cursor.Position, s);
        _cursor.MoveCursorBy(s.Length);
        OnContentChange();
    }

    private void RemoveAtCursor()
    {
        if (_cursor.Position >= Content.Length)
            return;

        Content.Remove(_cursor.Position, 1);
        OnContentChange();
    }

    private void RemoveBehindCursor()
    {
        if (_cursor.Position <= 0)
            return;

        _cursor.MoveCursorBy(-1);
        Content.Remove(_cursor.Position, 1);
        OnContentChange();
    }

    private void MoveCursorHorizontally(int amount)
    {
        var possibleAmount = Math.Clamp(amount, -_cursor.Position, Content.Length - _cursor.Position);
        if (possibleAmount == 0)
            return;

        _cursor.MoveCursorBy(possibleAmount);
    }

    private void MoveCursorVertically(int amount)
    {
        if (amount < 0 && _cursor.Position == 0 || amount > 0 && _cursor.Position == Content.Length)
            return;

        var cursorCoordinates = TextPositionToCoordinates(_cursor.Position);
        cursorCoordinates.Line += amount;
        var cursorPosition = TextCoordinatesToPosition(cursorCoordinates);
        _cursor.MoveCursorTo(cursorPosition);
    }

    private void OnContentChange()
    {
        _autoCommitter?.NotifyOfChange();
        CacheDrawText();
    }

    protected abstract bool IsValidCharacter(char input);

    protected virtual void HandleReturn()
    {
        StopTextInput();
    }

    protected virtual void HandleTab()
    {
    }

    protected abstract void CommitChange();

    public void OnLoseFocus()
    {
        StopTextInput();
    }

    private void StartTextInput()
    {
        if (IsReadOnly || _takingInput)
            return;

        _cursor.Enable();
        TextInputEXT.StartTextInput();
        TextInputEXT.TextInput += ParseInput;
        _takingInput = true;
    }

    private void StopTextInput()
    {
        if (!_takingInput)
            return;

        _cursor.Disable();
        TextInputEXT.StopTextInput();
        TextInputEXT.TextInput -= ParseInput;
        _takingInput = false;
        CommitChange();
    }

    protected override void UpdateElement(GameTime gameTime)
    {
        if (!_takingInput)
            return;

        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            StopTextInput();
            return;
        }

        foreach (var control in _cursorDirectionControl)
            control.Update(keyboardState, gameTime);

        _cursor.Update(gameTime, keyboardState, out var cursorMoved);
        _autoCommitter?.Update(gameTime, CommitChange);

        if (!cursorMoved)
            return;

        var cursorPosition = TextPositionToCoordinates(_cursor.Position);
        UpdateZoom(cursorPosition);
        _lastCursorCoordinates = cursorPosition;
    }

    private void UpdateZoom(TextCoordinates cursorCoordinates)
    {
        var cursorMovement = cursorCoordinates - _lastCursorCoordinates;
        if (cursorMovement is { Line: 0, Column: 0 })
            return;

        var characterSize = _font.MeasureString("*");
        var charactersInBounds = new Point((int)(Width / characterSize.X), (int)(Height / characterSize.Y));

        var columnOffset = cursorCoordinates.Column - _scrollOffset.Column;

        if (columnOffset > charactersInBounds.X)
        {
            _scrollOffset.Column += columnOffset - charactersInBounds.X;
        }

        const int leftScrollPadding = 3;

        if (columnOffset < 0)
        {
            _scrollOffset.Column += Math.Max(columnOffset - leftScrollPadding, -_scrollOffset.Column);
        }

        var lineOffset = cursorCoordinates.Line - _scrollOffset.Line;

        if (lineOffset >= charactersInBounds.Y)
        {
            _scrollOffset.Line += lineOffset - (charactersInBounds.Y - 1);
        }

        if (lineOffset < 0)
        {
            _scrollOffset.Line += lineOffset;
        }
    }

    private TextCoordinates TextPositionToCoordinates(int position)
    {
        var precedingText = _cachedText[..position];
        var line = 0;
        var lastReturn = -1;
        for (var i = 0; i < precedingText.Length; i++)
        {
            if (precedingText[i] != '\r')
                continue;

            line++;
            lastReturn = i;
        }

        return new TextCoordinates(line, position - (lastReturn + 1));
    }

    private int TextCoordinatesToPosition(TextCoordinates coordinates)
    {
        var line = coordinates.Line;

        switch (line)
        {
            case < 0:
                return 0;
            case 0:
                return GetPositionFromLine(0);
        }

        for (var i = 0; i < _cachedText.Length; i++)
        {
            if (_cachedText[i] != '\r')
                continue;

            if (--line > 0) continue;

            return GetPositionFromLine(i + 1);
        }

        return _cachedText.Length;

        int GetPositionFromLine(int lineStartIndex)
        {
            var cursorIndex = Math.Min(lineStartIndex + coordinates.Column, _cachedText.Length);
            var remainingText = _cachedText[lineStartIndex..cursorIndex];
            var newLineIndex = remainingText.IndexOf('\r');
            if (newLineIndex >= 0)
                return lineStartIndex + newLineIndex;

            return cursorIndex;
        }
    }

    protected override void DrawElement(DrawHelper drawHelper)
    {
        drawHelper.DrawSolid(Vector2.Zero, Width, Height, IsReadOnly ? Color.LightGray : Color.White);

        var textColor = Color.Black;

        var characterSize = _font.MeasureString("*");
        var charactersInBounds =
            new Point((int)(Width / characterSize.X), (int)(Height / characterSize.Y));

        if (charactersInBounds.X <= 0 || charactersInBounds.Y <= 0)
            return;

        var line = 0;
        var mustStillDrawCursor = _cursor.IsVisible;

        if (!string.IsNullOrEmpty(_cachedText))
        {
            var head = 0;

            for (var i = 0; i < _scrollOffset.Line; i++)
            {
                while (_cachedText[head++] != '\r' && head < _cachedText.Length)
                {
                }
            }

            var cursor = _cursor.IsVisible ? _cursor.Position : default(int?);

            while (head < _cachedText.Length)
            {
                var textOnLine = _cachedText[head.._cachedText.Length];

                var returnIndex = textOnLine.IndexOf('\r');
                if (returnIndex >= 0)
                {
                    textOnLine = textOnLine[..returnIndex];
                }

                var endIndex = Math.Min(_scrollOffset.Column + charactersInBounds.X, textOnLine.Length);
                if (endIndex >= _scrollOffset.Column)
                {
                    var textToDraw = textOnLine[_scrollOffset.Column..endIndex];

                    drawHelper.DrawText(_font, textToDraw, new Vector2(0, characterSize.Y * line), textColor);

                    if (head <= cursor && cursor <= head + textOnLine.Length)
                    {
                        drawHelper.DrawText(_font, "|", new Vector2(
                            (_lastCursorCoordinates.Column - _scrollOffset.Column - 0.4f) * characterSize.X,
                            characterSize.Y * line), textColor);
                        mustStillDrawCursor = false;
                    }
                }

                head += textOnLine.Length + 1;
                if (++line >= charactersInBounds.Y)
                    break;
            }
        }

        if (mustStillDrawCursor)
            drawHelper.DrawText(_font, "|", new Vector2(characterSize.X * -0.4f, characterSize.Y * line), textColor);
    }
}

public abstract class InputField<T> : InputField, IClickable
{
    protected abstract T Value { get; }

    public event Action<T>? Commit;

    protected InputField(SpriteFont font, float? autoCommitDelay = null)
        : base(font, autoCommitDelay)
    {
    }

    public void SetValue(T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        var stringValue = ValueToString(value);

        if (Content.ToString() == stringValue)
            return;

        Content.Clear();
        Content.Append(stringValue);
        CacheDrawText();

        CommitChange();
    }

    protected virtual string? ValueToString(T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return value.ToString();
    }

    protected override void CommitChange()
    {
        Commit?.Invoke(Value);
    }

    public abstract class InputFieldBuilder : UIElementBuilder<InputField<T>>
    {
        protected float? AutoCommitDelay { get; private set; }
        protected SpriteFont? Font { get; private set; }

        protected InputFieldBuilder()
        {
            AddValidationStep(() => Font != null, "Font has not been set.");
        }

        public InputFieldBuilder SetFont(SpriteFont font)
        {
            Font = font ?? throw new ArgumentNullException(nameof(font));
            return this;
        }

        public InputFieldBuilder AutoCommit(int delayInMilliseconds)
        {
            AutoCommitDelay = delayInMilliseconds;
            return this;
        }
    }
}