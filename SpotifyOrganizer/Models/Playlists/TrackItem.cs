using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.Playlists
{
    public class TrackItem
    {
        [JsonPropertyName("track")] public Track? Track { get; set; }
    }
}
