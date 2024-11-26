using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SensorServer.Models;

[Description("The direction in which to sort.")]
[JsonConverter(typeof(JsonStringEnumConverter<SortDirection>))]
public enum SortDirection
{
    [Description("Sort in ascending order.")]
    Ascending,
    [Description("Sort in descending order.")]
    Descending,
}
