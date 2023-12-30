using System.Runtime.CompilerServices;

namespace DyeLab.UI.InputField;

public struct TextCoordinates : IEquatable<TextCoordinates>
{
    public int Line { get; set; }
    public int Column { get; set; }

    public TextCoordinates(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public static TextCoordinates operator -(TextCoordinates value1, TextCoordinates value2)
    {
        return new TextCoordinates(value1.Line - value2.Line, value1.Column - value2.Column);
    }

    public override string ToString()
    {
        return "{Line:" + Line + " Column:" + Column + "}";
    }

    public bool Equals(TextCoordinates other)
    {
        return Line == other.Line && Column == other.Column;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextCoordinates other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Column);
    }
}