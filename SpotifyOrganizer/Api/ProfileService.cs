// Api/ProfileService.cs
using System.Threading.Tasks;
using SpotifyOrganizer.Models;

namespace SpotifyOrganizer.Api
{
    public class ProfileService
    {
        private readonly SpotifyApiClient _api;
        public ProfileService(SpotifyApiClient api) => _api = api;

        public Task<ProfileObject?> GetCurrentProfileAsync()
            => _api.GetAsync<ProfileObject>("https://api.spotify.com/v1/me");
    }
}
