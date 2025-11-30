using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;

namespace SpotifyOrganizer
{

    public static class SetUp
    {
        private const bool Verbose = false;
        static readonly List<string> skippable = new();
        static readonly Dictionary<string, List<(string, string)>> genres = new();
        // Artist ID -> genre (cache)
        static readonly Dictionary<string, string> artistGenreCache = new();
        // Artist Name -> genre (keeps behavior from original code which used name sometimes)
        static readonly Dictionary<string, string> artistNameLookup = new();

        // ---------- ENV & AUTH ----------
        private static async Task<(string clientId, string clientSecret, string redirectUri, string scope)> GetEnvVariables()
        {
            Env.Load("api.env");
            string clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";
            string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";
            string redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? "";

            // Fixed scopes (typo corrected)
            string scope = "playlist-modify-private playlist-modify-public user-read-private playlist-read-private playlist-read-collaborative";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            {
                Console.WriteLine("Missing CLIENT_ID, CLIENT_SECRET, or REDIRECT_URI in api.env; exiting.");
                Environment.Exit(1);
            }

            return (clientId, clientSecret, redirectUri, scope);
        }

        private static async Task<(string accessToken, string refreshToken)> PerformAuthentication((string clientId, string clientSecret, string redirectUri, string scope) EnvVariables)
        {
            var auth = new Authenticator(EnvVariables.clientId, EnvVariables.clientSecret, EnvVariables.redirectUri, EnvVariables.scope);

            string state = Guid.NewGuid().ToString("N");
            string authUrl = auth.GenerateAuthUrl(state);

            Console.WriteLine("Do we have permission to access your spotify profile? (y/n): ");
            string answer = Console.ReadLine() ?? "n";
            if (!string.Equals(answer.ToLower(), "y"))
            {
                Environment.Exit(1);
            }

            Console.WriteLine("Open this URL in your browser to authenticate:");
            Console.WriteLine(authUrl);

            Console.WriteLine("Waiting for Spotify authorization...");
            string code = auth.ListenForSpotifyAuth();
            Console.WriteLine("Got authorization!");

            var (accessToken, refreshToken) = await auth.ExchangeCodeForTokenAsync(code);
            return (accessToken, refreshToken);
        }

        // ---------- HELPERS: playlist detection, clearing, chunked add ----------
        private static async Task<string?> FindExistingPlaylistId(HttpClient http, string userId, string playlistName)
        {
            // Use user's playlist endpoint, paginated.
            string? next = $"https://api.spotify.com/v1/users/{userId}/playlists?limit=50";

            while (!string.IsNullOrEmpty(next))
            {
                var resp = await http.GetAsync(next);
                resp.EnsureSuccessStatusCode();
                var j = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(j);
                var items = doc.RootElement.GetProperty("items");

                foreach (var item in items.EnumerateArray())
                {
                    var name = item.GetProperty("name").GetString() ?? "";
                    if (string.Equals(name, playlistName, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.GetProperty("id").GetString();
                    }
                }

                var nextEl = doc.RootElement.GetProperty("next");
                next = nextEl.ValueKind == JsonValueKind.Null ? null : nextEl.GetString();
            }

            return null;
        }

        private static async Task ClearPlaylist(HttpClient http, string playlistId)
        {
            // Get track URIs for the playlist (only need uris)
            var getResp = await http.GetAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks?fields=items(track(uri)),total");
            getResp.EnsureSuccessStatusCode();
            var json = await getResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var items = doc.RootElement.GetProperty("items");
            if (items.GetArrayLength() == 0) return;

            // Build list of { "uri": "..."} objects for removal
            var tracks = items
                .EnumerateArray()
                .Select(i => new Dictionary<string, string> { ["uri"] = i.GetProperty("track").GetProperty("uri").GetString()! })
                .ToList();

            var body = new { tracks = tracks };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var req = new HttpRequestMessage(HttpMethod.Delete, $"https://api.spotify.com/v1/playlists/{playlistId}/tracks")
            {
                Content = content
            };

            var delResp = await http.SendAsync(req);
            delResp.EnsureSuccessStatusCode();
        }

        private static async Task AddTracksInChunks(HttpClient http, string playlistId, List<string> uris)
        {
            const int chunkSize = 100;
            for (int i = 0; i < uris.Count; i += chunkSize)
            {
                var chunk = uris.GetRange(i, Math.Min(chunkSize, uris.Count - i));
                var body = new { uris = chunk };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var resp = await http.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", content);
                resp.EnsureSuccessStatusCode();
            }
        }

        // ---------- GENRE RESOLUTION (cached) ----------
        public static async Task<string> GetTrackGenresAsync_Cached(Track track, string accessToken)
        {
            // Try by artist ID cache first
            foreach (var artist in track.Artists)
            {
                if (artistGenreCache.TryGetValue(artist.Id, out var cachedById))
                {
                    return cachedById;
                }
                if (artistNameLookup.TryGetValue(artist.Name, out var cachedByName))
                {
                    return cachedByName;
                }

                // fetch artist info
                var artistData = await Profile.GetSpotifyDataAsync<ArtistInfo>($"https://api.spotify.com/v1/artists/{artist.Id}", accessToken);
                if (artistData?.Genres != null && artistData.Genres.Count > 0)
                {
                    // take first genre; normalize to lowercase
                    var g = artistData.Genres[0].ToLowerInvariant();
                    artistGenreCache[artist.Id] = g;
                    artistNameLookup[artist.Name] = g;
                    return g;
                }
            }

            return "misc";
        }

        // ---------- PARSING PLAYLISTS INTO GENRES ----------
        static async Task ParsePlaylistDataSinglePass(SpotifyPlaylistsResponse playlistData, string accessToken)
        {
            Console.WriteLine("Parsing playlist data...");

            int totalPlaylists = playlistData.Items.Count;
            int current = 0;

            foreach (var playlist in playlistData.Items)
            {
                if (playlist.Tracks is null || playlist.Tracks.Href is null) continue;
                if (playlist.Tracks.Href == "" || playlist.Tracks.Href == null) continue;
                if (Verbose) Console.WriteLine($"Processing playlist: {playlist.Name} ({playlist.Tracks.Href})");

                // fetch tracks for this playlist
                var tracksData = await Profile.GetSpotifyDataAsync<PlaylistTracksResponse>(playlist.Tracks.Href, accessToken);
                if (tracksData?.Items == null) continue;

                current++;
                DrawProgressBar(current, totalPlaylists);

                foreach (var item in tracksData.Items)
                {
                    var t = item.Track;
                    if (t == null) continue;
                    string genre = (await GetTrackGenresAsync_Cached(t, accessToken)).ToLowerInvariant();

                    if (!genres.TryGetValue(genre, out var list))
                    {
                        list = new List<(string, string)>();
                        genres[genre] = list;
                    }

                    list.Add((t.Id, t.Name));

                    if (Verbose)
                    {
                        var artistNames = string.Join(", ", t.Artists.Select(a => a.Name));
                        Console.WriteLine($"   - {t.Name} by {artistNames} Genre: {genre}");
                    }
                }
            }

            DrawProgressBar(totalPlaylists, totalPlaylists);
            Console.WriteLine("\n✅ Parsing complete!");
            Console.WriteLine($"Number of genres found: {genres.Count}");
        }

        static void DrawProgressBar(int progress, int total, int barSize = 40)
        {
            if (total <= 0) return;
            double percent = (double)progress / total;
            int filled = (int)(percent * barSize);
            string bar = new string('█', filled) + new string(' ', barSize - filled);

            Console.CursorLeft = 0;
            Console.Write($"[{bar}] {progress}/{total} ({percent:P0})");
        }

        public static void DisplayGenresInPlaylists()
        {
            foreach (var pair in genres)
            {
                Console.WriteLine($"Playlist: {pair.Key}:");
                int count = 1;
                foreach ((string id, string name) in pair.Value)
                {
                    Console.WriteLine($"    {count}: {name}");
                    count++;
                }
            }
        }

        // ---------- PUBLISH: create or clear+repopulate ----------
        public static async Task PublishPlaylistsByGenre(string userId, string accessToken)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            foreach (var genre in genres)
            {
                string playlistName = $"Genre - {genre.Key}";

                // find existing playlist id (paginated)
                string? playlistId = await FindExistingPlaylistId(http, userId, playlistName);

                if (playlistId != null)
                {
                    Console.WriteLine($"🔄 Updating existing playlist: {playlistName}");
                    await ClearPlaylist(http, playlistId);
                }
                else
                {
                    Console.WriteLine($"🎧 Creating new playlist: {playlistName}");
                    var createBody = new
                    {
                        name = playlistName,
                        description = $"Auto-generated playlist for genre: {genre.Key}",
                        @public = true
                    };

                    var createContent = new StringContent(JsonSerializer.Serialize(createBody), Encoding.UTF8, "application/json");
                    var createResponse = await http.PostAsync($"https://api.spotify.com/v1/users/{userId}/playlists", createContent);
                    createResponse.EnsureSuccessStatusCode();

                    var createJson = await createResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(createJson);
                    playlistId = doc.RootElement.GetProperty("id").GetString();
                }

                // Convert to URIs and add in chunks
                var uris = genre.Value.Select(t => $"spotify:track:{t.Item1}").ToList();
                if (uris.Count > 0)
                {
                    await AddTracksInChunks(http, playlistId!, uris);
                }

                Console.WriteLine($"🎉 Playlist ready: {playlistName} ({uris.Count} tracks)");
            }
        }

        // ---------- USER PROMPTS ----------
        static void GetSkips()
        {
            Console.WriteLine("Which playlists to skip? (separate by commas), if none type N: ");
            string to_be_skipped = Console.ReadLine() ?? "N";
            if (to_be_skipped.ToLower().Equals("n")) return;
            string[] titles = to_be_skipped.Split(',');
            foreach (var title in titles) skippable.Add(title.Trim().ToLower());
        }

        // ---------- ORCHESTRATE ----------
        public static async Task<(ProfileObject?, SpotifyPlaylistsResponse?)> Orchestrate()
        {
            var EnvVariables = await GetEnvVariables();
            var (accessToken, refreshToken) = await PerformAuthentication(EnvVariables);

            // get profile + playlists
            var profileData = await Profile.GetSpotifyDataAsync<ProfileObject>("https://api.spotify.com/v1/me", accessToken);
            var playlistData = await Profile.GetSpotifyDataAsync<SpotifyPlaylistsResponse>("https://api.spotify.com/v1/me/playlists", accessToken);

            GetSkips();

            // Parse playlists (single pass, cached artist lookups)
            await ParsePlaylistDataSinglePass(playlistData!, accessToken);

            // Show genre groups
            DisplayGenresInPlaylists();

            // Publish playlists by genre (clear + repopulate)
            if (profileData is not null)
            {
                await PublishPlaylistsByGenre(profileData.Id, accessToken);
            }

            return (profileData, playlistData);
        }
    }
}
