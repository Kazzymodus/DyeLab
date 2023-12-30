using System.Reflection;
using Newtonsoft.Json;

namespace DyeLab.Configuration;

public class ConfigOptionJsonConverter<T> : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var objectType = value.GetType();
        if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof(ConfigOption<>))
            throw new JsonSerializationException();

        var configValue = (T?)objectType.InvokeMember(nameof(ConfigOption<T>.Value), BindingFlags.GetProperty, null, value, null);
        writer.WriteValue(configValue);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value != null && !reader.Value.GetType().IsAssignableTo(typeof(T)))
            throw new JsonSerializationException();

        objectType.InvokeMember(nameof(ConfigOption<T>.TrySet) , BindingFlags.InvokeMethod, null, existingValue, new[] { reader.Value });

        return existingValue;
    }

    public override bool CanConvert(Type objectType)
    {
        if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof(ConfigOption<>)) return false;
        var genericArguments = objectType.GetGenericArguments();
        if (genericArguments.Length != 1) return false;
        return genericArguments[0] == typeof(T);
    }
}