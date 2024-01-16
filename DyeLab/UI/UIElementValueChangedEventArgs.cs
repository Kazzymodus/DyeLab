namespace DyeLab.UI;

public class UIElementValueChangedEventArgs<T> : EventArgs
{
    public T NewValue { get; }
    public bool IsExternalChange { get; }

    public UIElementValueChangedEventArgs(T newValue, bool isExternalChange = false)
    {
        NewValue = newValue;
        IsExternalChange = isExternalChange;
    }
}