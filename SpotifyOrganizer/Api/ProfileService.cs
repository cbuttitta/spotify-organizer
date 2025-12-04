using SpotifyOrganizer.Models;

namespace SpotifyOrganizer.Api
{
    public class ProfileService
    {
        private readonly SpotifyApiClient _api;
        public ProfileService(SpotifyApiClient api) => _api = api; //create one instance of the api for all prifle requests

        public Task<ProfileObject?> GetCurrentProfileAsync()
            => _api.GetAsync<ProfileObject>("https://api.spotify.com/v1/me");
    }
}
