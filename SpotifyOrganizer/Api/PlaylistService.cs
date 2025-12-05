namespace SpotifyOrganizer.Api
{
    public class PlaylistService
    {
        private readonly SpotifyApiClient _api;
        public PlaylistService(SpotifyApiClient api) => _api = api;

        public async Task<string?> FindExistingPlaylistIdAsync(string userId, string playlistName)
        {
            string? next = $"https://api.spotify.com/v1/users/{userId}/playlists?limit=50";
            while (!string.IsNullOrEmpty(next))
            {
                var res = await _api.SendAsync(new HttpRequestMessage(HttpMethod.Get, next));
                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var items = doc.RootElement.GetProperty("items");
                foreach (var item in items.EnumerateArray()) //iterate over json items
                {
                    var name = item.GetProperty("name").GetString() ?? "";
                    if (string.Equals(name, playlistName, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.GetProperty("id").GetString();
                    }
                }
                var nextElement = doc.RootElement.GetProperty("next");
                next = nextElement.ValueKind == JsonValueKind.Null ? null : nextElement.GetString();
            }
            return null;
        }

        public async Task ClearPlaylistAsync(string playlistId)
        {
            var getResp = await _api.GetAsync<PlaylistTracksResponse>($"https://api.spotify.com/v1/playlists/{playlistId}/tracks?fields=items(track(uri)),total");
            if (getResp?.Items == null || getResp.Items.Count == 0) return;

            var tracks = getResp.Items
                .Select(i => new Dictionary<string, string> { ["uri"] = i.Track.Uri })
                .ToList();

            var body = new { tracks = tracks };
            var content = new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Delete, $"https://api.spotify.com/v1/playlists/{playlistId}/tracks") { Content = content };
            await _api.SendAsync(req);
        }

        public async Task AddTracksInChunksAsync(string playlistId, List<string> uris) //more efficient way of adding songs
        {
            const int chunkSize = 100;
            for (int i = 0; i < uris.Count; i += chunkSize)
            {
                var chunk = uris.GetRange(i, Math.Min(chunkSize, uris.Count - i));
                var body = new { uris = chunk };
                await _api.PostJsonAndReturnStringAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", body);
            }
        }

        public async Task<string> CreatePlaylistAsync(string userId, string playlistName, string description = "")
        {
            var body = new { name = playlistName, description = description, @public = true };
            string createJson = await _api.PostJsonAndReturnStringAsync($"https://api.spotify.com/v1/users/{userId}/playlists", body);
            using var doc = JsonDocument.Parse(createJson);
            return doc.RootElement.GetProperty("id").GetString() ?? throw new InvalidOperationException("Playlist id missing");
        }
    }
}
