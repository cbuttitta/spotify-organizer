// Api/GenreService.cs
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using SpotifyOrganizer.Models;

namespace SpotifyOrganizer.Api
{
    public class GenreService
    {
        private readonly SpotifyApiClient _api;
        // thread-safe cache in case we parallelize in future
        private readonly ConcurrentDictionary<string, string> _artistIdToGenre = new();

        public GenreService(SpotifyApiClient api) => _api = api;

        public async Task<string> GetTrackGenreCachedAsync(Track track)
        {
            foreach (var artist in track.Artists)
            {
                if (_artistIdToGenre.TryGetValue(artist.Id, out var cached)) return cached;

                var artistInfo = await _api.GetAsync<ArtistInfo>($"https://api.spotify.com/v1/artists/{artist.Id}");
                if (artistInfo?.Genres != null && artistInfo.Genres.Count > 0)
                {
                    var g = artistInfo.Genres[0].ToLowerInvariant();
                    _artistIdToGenre.TryAdd(artist.Id, g);
                    return g;
                }
            }
            return "misc";
        }
    }
}
