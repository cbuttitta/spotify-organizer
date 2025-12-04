using SpotifyOrganizer.Api;
using SpotifyOrganizer.Auth;
using SpotifyOrganizer.Models;
using SpotifyOrganizer.Utils;

namespace SpotifyOrganizer.Core
{
    public class Orchestrator
    {
        private readonly EnvironmentConfig _env;
        private readonly HttpClient _sharedHttp;
        private readonly SpotifyApiClient _apiClient;
        private readonly SpotifyAuthService _authService;
        private readonly ProfileService _profileService;
        private readonly PlaylistService _playlistService;
        private readonly GenreService _genreService;

        // internal storage for parsing playlists into genre buckets
        private readonly Dictionary<string, List<(string id, string name)>> _genres = new Dictionary<string, List<(string id, string name)>>();

        public Orchestrator(EnvironmentConfig env)
        {
            _env = env;
            _sharedHttp = new HttpClient();
            _apiClient = new SpotifyApiClient(_sharedHttp);
            _authService = new SpotifyAuthService(env.ClientId, env.ClientSecret, env.RedirectUri, env.Scope);
            _profileService = new ProfileService(_apiClient);
            _playlistService = new PlaylistService(_apiClient);
            _genreService = new GenreService(_apiClient);
        }

        public async Task RunAsync()
        {
            string state = Guid.NewGuid().ToString("N");
            string url = _authService.GenerateAuthUrl(state);

            bool allowed = ConsolePrompts.AskYesNo("Do we have permission to access your Spotify profile?", false);
            if (!allowed) Environment.Exit(1);

            Console.WriteLine("Open this URL in your browser to authenticate:");
            Console.WriteLine(url);
            Console.WriteLine("Waiting for Spotify authorization...");

            string code = _authService.ListenForCode();
            Console.WriteLine("Got authorization code.");

            var tokens = await _authService.ExchangeCodeForTokenAsync(code);
            _apiClient.SetAccessToken(tokens.AccessToken);

            var profile = await _profileService.GetCurrentProfileAsync();
            if (profile == null) throw new InvalidOperationException("Failed to load profile");

            Console.WriteLine($"Hi {profile.DisplayName ?? "Unknown User"}");

            // get playlists
            var playlistData = await _apiClient.GetAsync<SpotifyPlaylistsResponse>("https://api.spotify.com/v1/me/playlists");
            if (playlistData?.Items == null) { Console.WriteLine("No playlists found."); return; }

            // prompt for skips
            Console.WriteLine("Which playlists to skip? (separate by commas), if none type N: ");
            string tracksToSkip = Console.ReadLine() ?? "N";
            var skipSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase); //reduce duplicates
            if (!string.Equals(tracksToSkip.Trim().ToUpper(), "N"))
            {
                foreach (var s in tracksToSkip.Split(',').Select(x => x.Trim())) skipSet.Add(s);
            }

            // parse playlists -> genres
            int total = playlistData.Items.Count;
            int current = 0;
            foreach (var playlist in playlistData.Items)
            {
                current++;
                ProgressBar.Draw(current, total);

                if (playlist.Tracks?.Href == null) continue;
                if (skipSet.Contains(playlist.Name)) continue;

                var tracksData = await _apiClient.GetAsync<PlaylistTracksResponse>(playlist.Tracks.Href);
                if (tracksData?.Items == null) continue;

                foreach (var item in tracksData.Items)
                {
                    var trackObject = item.Track;
                    if (trackObject == null) continue;
                    string genre = (await _genreService.GetTrackGenreAsync(trackObject)).ToLowerInvariant();
                    if (!_genres.TryGetValue(genre, out var list))
                    {
                        list = new List<(string, string)>();
                        _genres[genre] = list;
                    }
                    _genres[genre].Add((trackObject.Id, trackObject.Name));
                }
            }

            ProgressBar.Draw(total, total);
            Console.WriteLine("\n Parsing complete!");
            Console.WriteLine($"Number of genres found: {_genres.Count}");


            // publish playlists
            foreach (var pair in _genres)
            {
                string playlistName = $"Genre - {pair.Key}";
                string? existingId = await _playlistService.FindExistingPlaylistIdAsync(profile.Id, playlistName);
                if (existingId != null)
                {
                    Console.WriteLine($"Updating existing playlist: {playlistName}");
                    await _playlistService.ClearPlaylistAsync(existingId);
                }
                else
                {
                    Console.WriteLine($"Creating new playlist: {playlistName}");
                    existingId = await _playlistService.CreatePlaylistAsync(profile.Id, playlistName, $"Auto-generated playlist for genre: {pair.Key}");
                }

                var uris = pair.Value.Select(t => $"spotify:track:{t.id}").ToList();
                if (uris.Count > 0)
                {
                    await _playlistService.AddTracksInChunksAsync(existingId, uris);
                }

                Console.WriteLine($"Playlist ready: {playlistName} ({uris.Count} tracks)");
            }
        }
    }
}
