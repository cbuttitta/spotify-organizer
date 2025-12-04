using System.Text.Json.Serialization;
namespace SpotifyOrganizer
{
    public class TrackItem
    {
        [JsonPropertyName("track")]
        public Track Track { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }
    }
}