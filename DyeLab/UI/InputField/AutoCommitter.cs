using Microsoft.Xna.Framework;

namespace DyeLab.UI.InputField;

public class AutoCommitter
{
    private TimeSpan _lastUpdated;
    private bool _resetLastUpdated;
    private bool _hasUncommittedChanges;
    private readonly float? _delayInSeconds;

    public AutoCommitter(float delayInSeconds)
    {
        _delayInSeconds = delayInSeconds;
    }

    public void Update(GameTime gameTime, Action<bool> commitDelegate)
    {
        if (_resetLastUpdated)
        {
            _lastUpdated = gameTime.TotalGameTime;
            _resetLastUpdated = false;
        }

        if (!_hasUncommittedChanges || (gameTime.TotalGameTime - _lastUpdated).TotalSeconds < _delayInSeconds)
            return;
        
        commitDelegate(false);
        
        _hasUncommittedChanges = false;
        _lastUpdated = gameTime.TotalGameTime;
    }

    public void NotifyOfChange()
    {
        _hasUncommittedChanges = true;
        _resetLastUpdated = true;
    }
}