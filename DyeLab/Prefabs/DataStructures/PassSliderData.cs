using DyeLab.Effects;

namespace DyeLab.Prefabs.DataStructures;

public class PassSliderData
{
    public PassSliderData(EffectWrapper activeEffect)
    {
        ActiveEffect = activeEffect;
    }

    public EffectWrapper ActiveEffect { get; }
}