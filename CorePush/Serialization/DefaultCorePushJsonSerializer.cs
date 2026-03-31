using System.Text.Json;

namespace CorePush.Serialization;

/// <summary>
/// Default JSON serializer using System.Text.Json with camelCase property naming.
/// Override <see cref="GetJsonSerializerOptions"/> to customize serialization behavior.
/// </summary>
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DefaultCorePushJsonSerializer : IJsonSerializer
{
    /// <inheritdoc />
    public string Serialize(object obj)
    {
        var options = GetJsonSerializerOptions();
        var json = JsonSerializer.Serialize(obj, options);

        return json;
    }

    /// <inheritdoc />
    public TObject Deserialize<TObject>(string json)
    {
        var options = GetJsonSerializerOptions();
        var obj = JsonSerializer.Deserialize<TObject>(json, options);

        return obj;
    }

    /// <summary>
    /// Returns the <see cref="JsonSerializerOptions"/> used for serialization and deserialization.
    /// Override this method to customize JSON behavior (e.g. custom converters, naming policies).
    /// </summary>
    protected virtual JsonSerializerOptions GetJsonSerializerOptions()
    {
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return settings;
    }
}
