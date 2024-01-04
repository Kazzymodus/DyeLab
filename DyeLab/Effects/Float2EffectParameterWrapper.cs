using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public sealed class Float2EffectParameterWrapper : VectorEffectParameterWrapper<Vector2>
{
    public Float2EffectParameterWrapper(string parameter)
        : base(parameter)
    {
    }

    public void SetX(float x)
    {
        SetComponent(x, (ref Vector2 v, float val) => v.X = val);
    }
    
    public void SetY(float y)
    {
        SetComponent(y, (ref Vector2 v, float val) => v.Y = val);
    }

    public override void Apply(EffectWrapper effect)
    {
        effect.Parameters[Parameter].SetValue(Value);
    }
}