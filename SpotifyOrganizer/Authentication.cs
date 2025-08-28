using System.Net;
using System.Web;
using System.Text;
using DotNetEnv;

namespace SpotifyOrganizer
{
    public class Authenticator(string clientId, string clientSecret, string redirectUri, string scope) //using primary constructor here for conciseness
    {
        //parameter values to be intialized once from .env file in main file
        public string GenerateAuthUrl(string state)
        {
            return $"https://accounts.spotify.com/authorize?client_id={clientId}" +
                   $"&response_type=code" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&scope={Uri.EscapeDataString(scope)}" +
                   $"&state={state}";
        }
        public string ListenForSpotifyAuth()
        {
            // Start local HTTP server to listen for the Spotify redirect
            var listener = new HttpListener();
            //Listen for the fed callback uri
            listener.Prefixes.Add(redirectUri.EndsWith('/') ? redirectUri : redirectUri + "/"); // add a slash if not present
            //Start the listener
            listener.Start();

            Console.WriteLine("Waiting for Spotify authorization...");

            // Wait for the incoming request (blocks thread until request is received)
            HttpListenerContext context = listener.GetContext();

            // Send response
            string responseString = "<html><body>You can close this window now.</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            string query = context.Request?.Url?.Query ?? string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                Console.WriteLine("No query string found in callback.");
                throw new Exception("No query string found in callback.");
            }

            // Parse query params safely
            var queryParams = System.Web.HttpUtility.ParseQueryString(query);

            string? error = queryParams.Get("error");
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error: {error}");
                throw new Exception($"Error: {error}");
            }
            string? code = queryParams.Get("code");
            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("No code found in callback.");
                listener.Stop();
                throw new InvalidOperationException("No code found in callback.");
            }

            listener.Stop();
            return code;
        }
        /* Exchange the authorization code for access and refresh tokens
         * Returns a tuple of (AccessToken, RefreshToken)

         */
        public async Task<(string AccessToken, string RefreshToken)> ExchangeCodeForTokenAsync(string code)
        {
            var client = new HttpClient(); //disposable client for making an HTTP request
            {
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    /*
                    * grant type: tell spotify we're exchanging an auth code
                    * the auth code I got from Spotify
                    * the uri I want the tokens to be sent to, that I'm listening for
                    * my client id
                    * my client secret
                    */
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code), //code spotify gave me
                    new KeyValuePair<string, string>("redirect_uri", redirectUri), //my uri I want spotify to send their tokens 
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                }); //POST request body

                //Send POST request and wait asynchronously for the server's reply
                var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);

                //get the JSON reponse and print it for debugging
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Token response: " + responseString);

                //parse the JSON and grab the root for further parsing of values
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                //isolate needed tokens
                string? accessToken = root.GetProperty("access_token").GetString();
                string? refreshToken = root.GetProperty("refresh_token").GetString();
                if (accessToken == null || refreshToken == null)
                {
                    Console.WriteLine("Access token or refresh token not found in response.");
                    throw new InvalidOperationException("Access token or refresh token not found in response.");
                }
                //return tokens to calling function
                return (accessToken, refreshToken);
            }
        }
    }
}