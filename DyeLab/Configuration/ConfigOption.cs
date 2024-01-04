using Newtonsoft.Json;
using ArgumentNullException = System.ArgumentNullException;

namespace DyeLab.Configuration;

[JsonConverter(typeof(ConfigOptionJsonConverter<string>))]
public sealed class ConfigOption<T>
{
    public T Value => _value ?? _default;

    public bool IsDefaultValue => EqualityComparer<T>.Default.Equals(Value, _default);

    private T? _value;
    private readonly T _default;

    public bool TrySet(T value)
    {
        if (!IsValidValue(value))
            return false;

        _value = value;
        return true;
    }

    public void Clear()
    {
        _value = default;
    }

    public ConfigOption(T defaultValue, params Func<T, bool>[] validators)
        : this(default, defaultValue, validators)
    {
    }

    public ConfigOption(T? value, T defaultValue, params Func<T, bool>[] validators)
    {
        if (defaultValue == null) throw new ArgumentNullException(nameof(defaultValue));
        _validators = validators;
        
        if (!IsValidValue(defaultValue))
            throw new ArgumentException($"The default value did not pass one or more validators.", nameof(defaultValue));

        _default = defaultValue;

        if (value != null && IsValidValue(value))
            _value = value;
    }

    private readonly ICollection<Func<T, bool>> _validators;

    private bool IsValidValue(T value)
    {
        return _validators.All(validator => validator(value));
    }
}