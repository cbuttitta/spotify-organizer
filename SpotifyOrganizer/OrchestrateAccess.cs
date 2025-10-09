using System.Threading.Tasks;
using DotNetEnv;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata;
using System.Text.Json;

namespace SpotifyOrganizer
{
    public class SetUp
    {
        static async Task<(string clientId, string clientSecret, string redirectUri, string scope)> GetEnvVariables()
        {
            Env.Load("api.env"); //load values from .env file
            string clientId = Environment.GetEnvironmentVariable("CLIENT_ID")!; //using '!' to promise compiler this will never be null
            string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;
            string redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI")!;
            string scope = "user-read-playback-state user-modify-playback-state playlist-modify-private user-read-private"; // PLACEHOLDER - MODIFY TO MEET MY PROGRAM's NEEDS
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
            Console.WriteLine("Open this URL in your browser:");
            Console.WriteLine(authUrl);

            // Step 2: Wait for redirect to my uri and extract the code Spotify gave me
            string code = auth.ListenForSpotifyAuth();
            //Console.WriteLine("Received code: " + code);

            // Step 3: Exchange code for tokens
            var (accessToken, refreshToken) = await auth.ExchangeCodeForTokenAsync(code);
            //Console.WriteLine("Access Token: " + accessToken);
            //Console.WriteLine("Refresh Token: " + refreshToken);
            return (accessToken, refreshToken);
        }
        static async Task<ProfileObject> GetProfile((string accessToken, string refreshToken) tokens)
        {
            //Access profile using authentication token and siplay response data
            string profile = await Profile.GetProfileInfo(tokens.accessToken);
            if (string.IsNullOrEmpty(profile))
            {
                System.Environment.Exit(1);
            }
            ProfileObject? profileData = JsonSerializer.Deserialize<ProfileObject>(profile);
            if (profileData != null)
            {
                //Console.WriteLine(profileData.DisplayName);
            }
            else
            {
                //Console.WriteLine("Failed to parse profile JSON.");
            }
            return profileData;
        }
        public static async Task<ProfileObject> Orchestrate()
        {
            var EnvVariables = await GetEnvVariables();
            var tokens = await PerformAuthentication(EnvVariables);
            var profileData = await GetProfile(tokens);
            return profileData;
        }
    }  
}