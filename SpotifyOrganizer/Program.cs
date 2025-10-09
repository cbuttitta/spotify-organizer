using System.Threading.Tasks;
using DotNetEnv;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata;
using System.Text.Json;



namespace SpotifyOrganizer
{

    public struct Song
    {
        public string Id;
        public string Genre;

        public Song(string id, string genre)
        {
            Id = id;
            Genre = genre;
        }
    }
    class Program
    {
        static void SongsToSort()
        {
            string[] genres = { "pop", "rnb", "rock", "jazz", "classical", "rap", "country", "hip-hop" };

            Song[] songs = Enumerable.Range(1, 30)
                .Select(i => new Song(i.ToString(), genres[(i - 1) % genres.Length])) // cycles through genres
                .ToArray();
            Sorter sorter = new Sorter(songs);
            sorter.SortGenre();
            sorter.PrintGenre();
        }

        static async Task Main(string[] args)
        {
            SongsToSort();
            var profileData = await SetUp.Orchestrate();
            string name = profileData.DisplayName ?? "Unknown User";
            Console.WriteLine($"Hi {name}");
            return;
        }
    }
}

