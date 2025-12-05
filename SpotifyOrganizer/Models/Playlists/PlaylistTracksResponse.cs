using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.Playlists
{
    public class PlaylistTracksResponse
    {
        [JsonPropertyName("items")] public List<TrackItem> Items { get; set; }
    }
}
