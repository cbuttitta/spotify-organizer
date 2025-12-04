using System.Text.Json.Serialization;
namespace SpotifyOrganizer
{
    public class SpotifyPlaylistsResponse
    {
        [JsonPropertyName("items")]
        public List<Playlist> Items { get; set; }
    }

    public class Playlist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("tracks")]
        public PlaylistTracks Tracks { get; set; }

        [JsonPropertyName("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }
    }

    public class PlaylistTracks
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class PlaylistTracksResponse
    {
        [JsonPropertyName("items")]
        public List<TrackItem> Items { get; set; }
    }


    public class ExternalUrls
    {
        [JsonPropertyName("spotify")]
        public string Spotify { get; set; }
    }
}