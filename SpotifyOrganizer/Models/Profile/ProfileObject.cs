using System.Text.Json.Serialization;
using SpotifyOrganizer.Models.General;

namespace SpotifyOrganizer.Models.Profile
{
    public class ProfileObject
    {
        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
        [JsonPropertyName("explicit_content")] public ExplicitContent? ExplicitContent { get; set; }
        [JsonPropertyName("external_urls")] public ExternalUrls? ExternalUrls { get; set; }
        [JsonPropertyName("followers")] public Followers? Followers { get; set; }
        [JsonPropertyName("href")] public string? Href { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("images")] public List<ImageObject>? Images { get; set; }
        [JsonPropertyName("product")] public string? Product { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("uri")] public string? Uri { get; set; }
    }
}
