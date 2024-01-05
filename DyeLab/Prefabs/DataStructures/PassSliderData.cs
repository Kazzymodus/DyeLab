using DyeLab.Effects;
using DyeLab.Effects.Constants;

namespace DyeLab.Prefabs.DataStructures;

public class PassSliderData
{
    public PassSliderData(EffectWrapper activeEffect, Action<EffectType> setActiveEffectDelegate)
    {
        ActiveEffect = activeEffect;
        SetActiveEffectDelegate = setActiveEffectDelegate;
    }

    public EffectWrapper ActiveEffect { get; }
    public Action<EffectType> SetActiveEffectDelegate { get; }
}