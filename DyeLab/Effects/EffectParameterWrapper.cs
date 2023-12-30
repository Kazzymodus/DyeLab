using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public abstract class EffectParameterWrapper
{
    protected string Parameter { get; }

    protected EffectParameterWrapper(string parameter)
    {
        Parameter = parameter;
    }

    public abstract void Apply(Effect effect);
}

public abstract class EffectParameterWrapper<T> : EffectParameterWrapper
{
    protected T? Value { get; set; }

    protected EffectParameterWrapper(string parameter)
        : base(parameter)
    {
    }

    public void Set(T value)
    {
        Value = value;
    }
}