using DyeLab.Effects.Constants;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public class EffectChangedEventArgs : EventArgs
{
    public Effect NewEffect { get; }

    public EffectChangedEventArgs(Effect newEffect)
    {
        NewEffect = newEffect;
    }
}