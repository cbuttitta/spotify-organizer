using SpotifyOrganizer.Models;

namespace SpotifyOrganizer.Api
{
    public class GenreService
    {
        private readonly SpotifyApiClient _api;

        private readonly Dictionary<string, string> _artistGenre = new();

        public GenreService(SpotifyApiClient api)
        {
            _api = api;
        }
        public async Task<string> GetTrackGenreAsync(Track track)
        {
            foreach (var artist in track.Artists)
            {
                //check if genre already associated with this artist
                if (_artistGenre.TryGetValue(artist.Id, out var genreFound)) //passes value found to variable genreFound
                {
                    return genreFound;
                }

                //if not, figure out the genre and store it for futurre reference
                var artistInfo =
                    await _api.GetAsync<ArtistInfo>($"https://api.spotify.com/v1/artists/{artist.Id}");

                if (artistInfo != null && artistInfo.Genres != null && artistInfo.Genres.Count > 0)
                {
                    var genre = artistInfo.Genres[0].ToLower();

                    // Store in dicitonary
                    _artistGenre[artist.Id] = genre;

                    return genre;
                }
            }

            // No genre for artist -> misc
            return "misc";
        }
    }
}
