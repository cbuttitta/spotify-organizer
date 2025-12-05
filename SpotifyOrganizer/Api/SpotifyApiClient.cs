using System.Net.Http.Headers;
using System.Text.Json;

namespace SpotifyOrganizer.Api
{
    public class SpotifyApiClient
    {
        private readonly HttpClient _http;

        public SpotifyApiClient(HttpClient httpClient)
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void SetAccessToken(string accessToken)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            var resp = await _http.GetAsync(url);
            var json = await resp.Content.ReadAsStringAsync();
            resp.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<string> PostJsonAndReturnStringAsync(string url, object body)
        {
            var jsonBody = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(url, content);
            var text = await resp.Content.ReadAsStringAsync();
            resp.EnsureSuccessStatusCode(); //throw exception if resposne is anything but 200
            return text;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
        {
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return resp;
        }
    }
}
