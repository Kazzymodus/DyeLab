using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public sealed class Float3EffectParameterWrapper : VectorEffectParameterWrapper<Vector3>
{
    public Float3EffectParameterWrapper(string parameter)
        : base(parameter)
    {
    }

    public void SetX(float x)
    {
        SetComponent(x, (ref Vector3 v, float val) => v.X = val);
    }
    
    public void SetY(float y)
    {
        SetComponent(y, (ref Vector3 v, float val) => v.Y = val);
    }
    
    public void SetZ(float z)
    {
        SetComponent(z, (ref Vector3 v, float val) => v.Z = val);
    }

    public override void Apply(EffectWrapper effect)
    {
        effect.Parameters[Parameter].SetValue(Value);
    }
}