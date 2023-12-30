using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public sealed class Float4EffectParameterWrapper : VectorEffectParameterWrapper<Vector4>
{
    public Float4EffectParameterWrapper(string parameter)
        : base(parameter)
    {
    }

    public void SetX(float x)
    {
        SetComponent(x, (ref Vector4 v, float val) => v.X = val);
    }
    
    public void SetY(float y)
    {
        SetComponent(y, (ref Vector4 v, float val) => v.Y = val);
    }
    
    public void SetZ(float z)
    {
        SetComponent(z, (ref Vector4 v, float val) => v.Z = val);
    }
        
    public void SetW(float w)
    {
        SetComponent(w, (ref Vector4 v, float val) => v.W = val);
    }

    public override void Apply(Effect effect)
    {
        effect.Parameters[Parameter].SetValue(Value);
    }
}