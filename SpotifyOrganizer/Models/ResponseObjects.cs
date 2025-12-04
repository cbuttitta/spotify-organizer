// Models/ResponseObjects.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models
{
    #region Profile
    public class ProfileObject
    {
        [JsonPropertyName("country")] public string Country { get; set; }
        [JsonPropertyName("display_name")] public string DisplayName { get; set; }
        [JsonPropertyName("explicit_content")] public ExplicitContent ExplicitContent { get; set; }
        [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }
        [JsonPropertyName("followers")] public Followers Followers { get; set; }
        [JsonPropertyName("href")] public string Href { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("images")] public List<ImageObject> Images { get; set; }
        [JsonPropertyName("product")] public string Product { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("uri")] public string Uri { get; set; }
    }

    public class ImageObject
    {
        [JsonPropertyName("url")] public string Url { get; set; }
        [JsonPropertyName("height")] public int? Height { get; set; }
        [JsonPropertyName("width")] public int? Width { get; set; }
    }

    public class ExplicitContent
    {
        [JsonPropertyName("filter_enabled")] public bool FilterEnabled { get; set; }
        [JsonPropertyName("filter_locked")] public bool FilterLocked { get; set; }
    }
    #endregion

    public class Followers
    {
        [JsonPropertyName("href")] public string Href { get; set; }
        [JsonPropertyName("total")] public int Total { get; set; }
    }

    #region Playlists
    public class SpotifyPlaylistsResponse
    {
        [JsonPropertyName("items")] public List<Playlist> Items { get; set; }
    }

    public class Playlist
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("tracks")] public PlaylistTracks Tracks { get; set; }
        [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }
    }

    public class PlaylistTracks
    {
        [JsonPropertyName("href")] public string Href { get; set; }
        [JsonPropertyName("total")] public int Total { get; set; }
    }

    public class PlaylistTracksResponse
    {
        [JsonPropertyName("items")] public List<TrackItem> Items { get; set; }
    }

    public class TrackItem
    {
        [JsonPropertyName("track")] public Track Track { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("artists")] public List<Artist> Artists { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("href")] public string Href { get; set; }
        // spotify sometimes returns uri in track object, include for ClearPlaylist logic
        [JsonPropertyName("uri")] public string Uri { get; set; }
    }

    public class Artist
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("href")] public string Href { get; set; }
        [JsonPropertyName("uri")] public string Uri { get; set; }
        [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }
    }
    #endregion

    #region Artist
    public class ArtistInfo
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("genres")] public List<string> Genres { get; set; }
    }
    #endregion

    #region General
    public class ExternalUrls
    {
        [JsonPropertyName("spotify")] public string Spotify { get; set; }
    }
    #endregion
}
