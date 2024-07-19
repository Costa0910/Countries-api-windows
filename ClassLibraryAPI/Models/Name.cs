using System.Text.Json.Serialization;

namespace ClassLibraryAPI.Models;

public class Name
{
    public string Common { get; set; }

    public string Official { get; set; }

    [JsonIgnore]
    public object NativeName { get; set; }
}