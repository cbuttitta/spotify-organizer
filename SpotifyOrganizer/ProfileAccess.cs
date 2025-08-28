using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyOrganizer
{
    public class Profile
    {
        private static readonly HttpClient _httpClient = new HttpClient(); //static and reusable across instances
        public static async Task<string> DisplayProfileName(string token)
        {
            _httpClient.DefaultRequestHeaders.Clear(); //clear any existing headers
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://api.spotify.com/v1/me");

                response.EnsureSuccessStatusCode(); // Throw if not a success code

                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return "";
            }
        }
    }
}