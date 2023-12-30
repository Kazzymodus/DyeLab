using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public sealed class FloatEffectParameterWrapper : EffectParameterWrapper<float>
    { 
        public FloatEffectParameterWrapper(string parameter)
            : base(parameter)
        {
        }

        public override void Apply(Effect effect)
        {
            effect.Parameters[Parameter].SetValue(Value);
        }
    }