using System;
using System.Net;
using System.Web;
using DotNetEnv;


namespace HelloWorld
{
    class Program
    {
        static void listenForSpotifyAuth(string uri)
        {
            // Start local HTTP server to listen for the Spotify redirect
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(redirectUri + "/");
            listener.Start();

            Console.WriteLine("Waiting for Spotify authorization...");

            // Wait for the incoming request
            HttpListenerContext context = listener.GetContext();
            string responseString = "<html><body>You can close this window now.</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            // Extract "code" from the query string
            var query = context.Request.Url.Query;
            var queryParams = HttpUtility.ParseQueryString(query);
            string code = queryParams.Get("code");

            Console.WriteLine("Authorization code: " + code);

            listener.Stop();
        }
        static void authenticateSpotify()
        {
            Env.load();
            string clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            string redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI");
            string scope = "user-read-playback-state user-modify-playback-state playlist-modify-private";
            string authUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}" +
                 $"&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                 $"&scope={Uri.EscapeDataString(scope)}";
            Console.WriteLine("Open this URL in your browser to authorize:");
            Console.WriteLine(authUrl);
            listenForSpotifyAuth(redirectUri);
        }
        static void Main(string[] args)
        {
            authenticateSpotify();
        }
    }
}

