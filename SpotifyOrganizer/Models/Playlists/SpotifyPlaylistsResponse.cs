using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.Playlists
{
    public class SpotifyPlaylistsResponse
    {
        [JsonPropertyName("items")] public List<Playlist> Items { get; set; }
    }
}
