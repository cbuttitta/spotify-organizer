using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.General
{
    public class Followers
    {
        [JsonPropertyName("href")] public string Href { get; set; }
        [JsonPropertyName("total")] public int Total { get; set; }
    }
}
