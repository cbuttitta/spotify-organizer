using System.Text.Json.Serialization;
using SpotifyOrganizer.Models.General;

namespace SpotifyOrganizer.Models.Playlists
{
    public class Playlist
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("tracks")] public PlaylistTracks Tracks { get; set; }
        [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }
    }
}
