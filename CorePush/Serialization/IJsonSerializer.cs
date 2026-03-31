namespace CorePush.Serialization;

/// <summary>
/// Abstraction for JSON serialization, allowing consumers to plug in a custom serializer
/// (e.g. Newtonsoft.Json) instead of the default System.Text.Json implementation.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    string Serialize(object obj);

    /// <summary>
    /// Deserializes a JSON string into an object of the specified type.
    /// </summary>
    /// <typeparam name="TObject">The target type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    TObject Deserialize<TObject>(string json);
}
