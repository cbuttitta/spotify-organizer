using SpotifyOrganizer.Core;

namespace SpotifyOrganizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var env = EnvironmentConfig.Load("api.env");
                var runner = new Orchestrator(env);
                await runner.RunAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled error: " + e.Message);
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }
        }
    }
}
