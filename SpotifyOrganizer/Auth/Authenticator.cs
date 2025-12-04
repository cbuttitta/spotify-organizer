// Auth/Authenticator.cs
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace SpotifyOrganizer.Auth
{
    public class Authenticator
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _scope;

        public Authenticator(string clientId, string clientSecret, string redirectUri, string scope)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _scope = scope;
        }

        public string GenerateAuthUrl(string state)
        {
            var encodedRedirect = Uri.EscapeDataString(_redirectUri);
            var encodedScope = Uri.EscapeDataString(_scope);
            return $"https://accounts.spotify.com/authorize?client_id={_clientId}" +
                   $"&response_type=code" +
                   $"&redirect_uri={encodedRedirect}" +
                   $"&scope={encodedScope}" +
                   $"&state={state}";
        }

        // Blocking listen (keeps original semantics) — returns code or throws
        public string ListenForSpotifyAuth()
        {
            using var listener = new HttpListener();
            string prefix = _redirectUri.EndsWith("/") ? _redirectUri : _redirectUri + "/";
            listener.Prefixes.Add(prefix);
            listener.Start();

            HttpListenerContext context = listener.GetContext();

            string responseString = "<html><body>Authentication success. You can close this window now.</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            string query = context.Request?.Url?.Query ?? string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                listener.Stop();
                throw new InvalidOperationException("No query string found in callback.");
            }

            var queryParams = HttpUtility.ParseQueryString(query);
            string? error = queryParams.Get("error");
            if (!string.IsNullOrEmpty(error))
            {
                listener.Stop();
                throw new InvalidOperationException($"Spotify error: {error}");
            }

            string? code = queryParams.Get("code");
            if (string.IsNullOrEmpty(code))
            {
                listener.Stop();
                throw new InvalidOperationException("No code found in callback.");
            }

            listener.Stop();
            return code;
        }
    }
}
