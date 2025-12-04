using System.Text.Json;

namespace SpotifyOrganizer.Auth
{
    public class TokenResult
    {
        public string AccessToken { get; init; } = "";
        public string RefreshToken { get; init; } = "";
        public int ExpiresIn { get; init; }
    }

    public class SpotifyAuthService
    {
        private readonly Authenticator _authenticator;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public SpotifyAuthService(string clientId, string clientSecret, string redirectUri, string scope)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _authenticator = new Authenticator(clientId, clientSecret, redirectUri, scope);
        }

        public string GenerateAuthUrl(string state) => _authenticator.GenerateAuthUrl(state);

        public string ListenForCode() => _authenticator.ListenForSpotifyAuth();

        public async Task<TokenResult> ExchangeCodeForTokenAsync(string code)
        {
            using var client = new HttpClient();
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","authorization_code"),
                new KeyValuePair<string,string>("code", code),
                new KeyValuePair<string,string>("redirect_uri", _redirectUri),
                new KeyValuePair<string,string>("client_id", _clientId),
                new KeyValuePair<string,string>("client_secret", _clientSecret)
            });

            var resp = await client.PostAsync("https://accounts.spotify.com/api/token", body);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Token exchange failed: {resp.StatusCode} - {json}");
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? accessToken = root.GetProperty("access_token").GetString();
            string? refreshToken = root.GetProperty("refresh_token").GetString();

            if (accessToken == null)
                throw new InvalidOperationException("Access token missing from token response.");

            return new TokenResult { AccessToken = accessToken, RefreshToken = refreshToken ?? ""};
        }
    }
}
