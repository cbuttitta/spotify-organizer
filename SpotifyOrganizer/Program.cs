using System.Threading.Tasks;
using DotNetEnv;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata;
using System.Text.Json;



namespace SpotifyOrganizer
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var (profileData, playlistData) = await SetUp.Orchestrate();
            string name = profileData.DisplayName ?? "Unknown User";
            Console.WriteLine($"Hi {name}");
            //Console.WriteLine("Your Playlists: ");
            return;
        }
    }
}

