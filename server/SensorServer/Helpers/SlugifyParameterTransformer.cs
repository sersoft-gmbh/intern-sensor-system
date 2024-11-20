using System.Text.RegularExpressions;

namespace SensorServer.Helpers;

public sealed partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex ParameterSlug { get; }

    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;
        var stringValue = value.ToString();
        return stringValue == null ? null : ParameterSlug.Replace(stringValue, "$1-$2").ToLower();
    }
}
