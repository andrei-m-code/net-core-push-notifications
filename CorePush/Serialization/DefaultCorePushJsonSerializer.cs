using System.Text.Json;

namespace CorePush.Serialization;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DefaultCorePushJsonSerializer : IJsonSerializer
{
    public string Serialize(object obj)
    {
        var options = GetJsonSerializerOptions();
        var json = JsonSerializer.Serialize(obj, options);
            
        return json;
    }

    public TObject Deserialize<TObject>(string json)
    {
        var options = GetJsonSerializerOptions();
        var obj = JsonSerializer.Deserialize<TObject>(json, options);

        return obj;
    }

    protected virtual JsonSerializerOptions GetJsonSerializerOptions()
    {
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
            
        return settings;
    }
}