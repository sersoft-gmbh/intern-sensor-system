using System.Text.RegularExpressions;

namespace SensorServer.Helpers;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;
        var stringValue = value.ToString();
        return stringValue == null ? null : Regex.Replace(stringValue, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
