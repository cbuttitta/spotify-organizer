using DotNetEnv;

namespace SpotifyOrganizer.Core
{
    public class EnvironmentConfig
    {
        //Initialize getters
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string RedirectUri { get; }
        public string Scope { get; }

        //build env variables into constructor
        private EnvironmentConfig(string clientId, string clientSecret, string redirectUri, string scope)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
            Scope = scope;
        }

        public static EnvironmentConfig Load(string envFile = "api.env")
        {
            Env.Load(envFile);
            string clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";
            string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";
            string redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? "";

            string scope = "playlist-modify-private playlist-modify-public user-read-private playlist-read-private playlist-read-collaborative";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            {
                Console.WriteLine("Missing CLIENT_ID, CLIENT_SECRET, or REDIRECT_URI in api.env; exiting.");
                Environment.Exit(1);
            }

            //return constrctor 
            return new EnvironmentConfig(clientId, clientSecret, redirectUri, scope);
        }
    }
}
