namespace DyeLab.Effects;

public abstract class VectorEffectParameterWrapper<T> : EffectParameterWrapper<T> where T : struct
{
    protected VectorEffectParameterWrapper(string parameter)
        : base(parameter)
    {
            
    }
        
    protected delegate void SetVectorComponent(ref T vector, float value);
        
    protected void SetComponent(float f, SetVectorComponent component)
    {
        var value = Value;
        component(ref value, f);
        Value = value;
    }
}