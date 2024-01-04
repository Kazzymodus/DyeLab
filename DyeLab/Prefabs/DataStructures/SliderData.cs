using Microsoft.Xna.Framework;

namespace DyeLab.Prefabs.DataStructures;

public class SliderData
{
    public SliderData(float minValue, float maxValue, float startValue, Color? barColor = null)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        StartValue = startValue;
        BarColor = barColor;
    }

    public float MinValue { get; }
    public float MaxValue { get; }
    public float StartValue { get; }
    public Color? BarColor { get; }
}