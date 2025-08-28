using System;
using System.Threading.Tasks;
using DotNetEnv;
using SpotifyAuth; // Import the Authenticator class
using ProfileAccess;



namespace SpotifyOrganizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Env.Load(); //load values from .env file
            string clientId = Environment.GetEnvironmentVariable("CLIENT_ID")!; //using '!' to promise compiler this will never be null
            string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;
            string redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI")!;
            string scope = "user-read-playback-state user-modify-playback-state playlist-modify-private user-read-private"; // PLACEHOLDER - MODIFY TO MEET MY PROGRAM's NEEDS



            var auth = new Authenticator(clientId, clientSecret, redirectUri, scope);

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
            Console.WriteLine("Received code: " + code);

            // Step 3: Exchange code for tokens
            var (accessToken, refreshToken) = await auth.ExchangeCodeForTokenAsync(code);
            Console.WriteLine("Access Token: " + accessToken);
            Console.WriteLine("Refresh Token: " + refreshToken);

            //Access profile using authentication token and siplay response data
            string profile = await Profile.DisplayProfileName(accessToken);
            Console.WriteLine(profile);
            return;
        }
    }
}

