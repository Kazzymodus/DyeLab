using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.Effects;

public class EffectWrapper
{
    private Effect _effect;
    private int _passIndex;

    public event Action<Effect>? EffectChanged;

    public EffectParameterCollection Parameters => _effect.Parameters;
    public EffectPass CurrentPass => _effect.CurrentTechnique.Passes[_passIndex];
    public EffectPassCollection Passes => _effect.CurrentTechnique.Passes;

    public EffectWrapper(Effect effect)
    {
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
    }

    public void SetEffect(Effect effect)
    {
        if (_effect == effect)
            return;

        _effect = effect;
        _passIndex = 0;

        EffectChanged?.Invoke(effect);
    }

    public void SetPassIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must not be negative");

        if (index >= _effect.CurrentTechnique.Passes.Count)
            throw new ArgumentOutOfRangeException(nameof(index), index,
                $"Index must be lower than the amount of passes ({_effect.CurrentTechnique.Passes.Count})");

        _passIndex = index;
    }

    public void Apply()
    {
        _effect.CurrentTechnique.Passes[_passIndex].Apply();
    }
}