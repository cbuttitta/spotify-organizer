using System.Threading.Tasks;
using DotNetEnv;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel; //for List

namespace SpotifyOrganizer
{
    public class SetUp
    {
        private const bool Verbose = false; //whether or not to print data as it is being processed
        //static List<string> skippable;
        static Dictionary<string, List<(string, string)>> genres = new Dictionary<string, List<(string, string)>>();
        static Dictionary<string, string> artistLookup = new Dictionary<string, string>(); // prevent duplicate lookups
        static async Task<(string clientId, string clientSecret, string redirectUri, string scope)> GetEnvVariables()
        {
            Env.Load("api.env"); //load values from .env file
            string clientId = Environment.GetEnvironmentVariable("CLIENT_ID")!; //using '!' to promise compiler this will never be null
            string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;
            string redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI")!;
            string scope = "playlist-modify-private user-read-private playlist-read-private playlist-read-collaborative"; // PLACEHOLDER - MODIFY TO MEET MY PROGRAM's NEEDS
            if (string.IsNullOrEmpty(clientId))
            {
                Console.WriteLine("Could not retrieve the client id; exiting.");
                System.Environment.Exit(1);
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                Console.WriteLine("Could not retrieve the client secret; exiting.");
                System.Environment.Exit(1);
            }
            if (string.IsNullOrEmpty(redirectUri))
            {
                Console.WriteLine("Could not retrieve the redirect uri; exiting.");
                System.Environment.Exit(1);
            }
            return (clientId, clientSecret, redirectUri, scope);


        }
        static async Task<(string accessToken, string refreshToken)> PerformAuthentication((string clientId, string clientSecret, string redirectUri, string scope) EnvVariables)
        {
            var auth = new Authenticator(EnvVariables.clientId, EnvVariables.clientSecret, EnvVariables.redirectUri, EnvVariables.scope);

            /* Generate a unique random 32 character string for the 'state' value of authentication, 
             * Spotify requires this to prevent CSRF attacks
             */
            string state = Guid.NewGuid().ToString("N");

            // Step 1: Ask user to log in
            string authUrl = auth.GenerateAuthUrl(state);
            Console.WriteLine("Do we have permission to access your spotify profile? (y/n): ");
            string answer = Console.ReadLine()!;
            if (!string.Equals(answer.ToLower(), "y"))
            {
                System.Environment.Exit(1);
            }
            Console.WriteLine("Open this URL in your browser to authenticate:");
            Console.WriteLine(authUrl);

            // Step 2: Wait for redirect to my uri and extract the code Spotify gave me
            string code = auth.ListenForSpotifyAuth();
            Console.WriteLine("Waiting for Spotify authorization...");
            Console.WriteLine("Got authorization!");

            // Step 3: Exchange code for tokens
            var (accessToken, refreshToken) = await auth.ExchangeCodeForTokenAsync(code);
            //Console.WriteLine("Access Token: " + accessToken);
            //Console.WriteLine("Refresh Token: " + refreshToken);
            return (accessToken, refreshToken);
        }
        static async Task ParsePlaylistData(SpotifyPlaylistsResponse playlistData, string accessToken)
        {
            Console.WriteLine("Parsing playlist data...");

            int totalPlaylists = playlistData.Items.Count;
            int current = 0;

            foreach (var playlist in playlistData.Items)
            {
                current++;

                // Update progress bar before processing
                DrawProgressBar(current, totalPlaylists);

                if (Verbose) Console.WriteLine($"\n🎵 Playlist: {playlist.Name} ({playlist.Tracks.Total} tracks)");

                var tracksData = await Profile.GetSpotifyDataAsync<PlaylistTracksResponse>(
                    playlist.Tracks.Href,
                    accessToken
                );

                if (tracksData?.Items != null)
                {
                    foreach (var item in tracksData.Items)
                    {
                        string genre = await GetTrackGenresAsync(item.Track, accessToken);
                        if (genres.ContainsKey(genre))
                        {
                            genres[genre].Add((item.Track.Id, item.Track.Name));
                        }
                        else
                        {
                            genres.Add(genre, new List<(string, string)> { (item.Track.Id, item.Track.Name) });
                        }

                        string artistNames = string.Join(", ", item.Track.Artists.ConvertAll(a => a.Name));
                        if (Verbose) Console.WriteLine($"   - {item.Track.Name} by {artistNames} Genre: {genre}");
                    }
                }
            }

            // Finish progress bar
            DrawProgressBar(totalPlaylists, totalPlaylists);
            Console.WriteLine("\n✅ Parsing complete!");
        }
        static void DrawProgressBar(int progress, int total, int barSize = 40)
        {
            double percent = (double)progress / total;
            int filled = (int)(percent * barSize);
            string bar = new string('█', filled) + new string(' ', barSize - filled);

            Console.CursorLeft = 0; // Overwrite current line
            Console.Write($"[{bar}] {progress}/{total} ({percent:P0})");
        }

        public static async Task<string> GetTrackGenresAsync(Track track, string accessToken)
        {
            Artist artist = track.Artists[0];
            if (artistLookup.ContainsKey(artist.Name))
            {
                return artistLookup[artist.Name];
            }
            var artistData = await Profile.GetSpotifyDataAsync<ArtistInfo>($"https://api.spotify.com/v1/artists/{artist.Id}", accessToken);
            string genre;
            if (artistData?.Genres != null && artistData.Genres.Count > 0)
            {
                genre = artistData.Genres[0];
            }
            else
            {
                return "";
            }
            return genre;
        }
        public static async Task DisplayGenres()
        {
            foreach (KeyValuePair<string, List<(string, string)>> pair in genres)
            {
                string songs = string.Join(", ", pair.Value[0]); // join the list into a string
                Console.WriteLine($"{pair.Key}: IDs: {songs}");
            }
        }
        public static async Task DisplayGenresInPlaylists()
        {
            foreach (KeyValuePair<string, List<(string, string)>> pair in genres)
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
        /*static void GetSkips()
        {
            Console.WriteLine("Which playlists to skip? (seperate by commas), if none type N: ");
            string to_be_skipped = Console.ReadLine()!;
            if (to_be_skipped.ToLower().Equals("N"))
            {
                return;
            }
            string[] titles = to_be_skipped.Split(',');
            foreach (var title in titles)
            {
                skippable.Add(title);
            }

        }*/
        public static async Task<(ProfileObject?, SpotifyPlaylistsResponse?)> Orchestrate()
        {
            var EnvVariables = await GetEnvVariables();
            var (accessToken, refreshToken) = await PerformAuthentication(EnvVariables);
            var profileData = await Profile.GetSpotifyDataAsync<ProfileObject>("https://api.spotify.com/v1/me", accessToken);
            var playlistData = await Profile.GetSpotifyDataAsync<SpotifyPlaylistsResponse>("https://api.spotify.com/v1/me/playlists", accessToken);
            //GetSkips(); implement later
            await ParsePlaylistData(playlistData, accessToken);
            await DisplayGenresInPlaylists();
            return (profileData, playlistData);
        }
    }
}