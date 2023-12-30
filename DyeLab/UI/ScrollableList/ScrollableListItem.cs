namespace DyeLab.UI.ScrollableList;

public class ScrollableListItem<T>
{
    public ScrollableListItem(string label, T value)
    {
        Label = label;
        Value = value;
    }
    
    public string Label { get; }
    public T Value { get; }
}