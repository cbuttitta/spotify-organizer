using SpotifyOrganizer.Core;
namespace SpotifyOrganizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var env = EnvironmentConfig.Load("api.env"); //gte environemnt variables
                var runner = new Orchestrator(env); //instantiate the orchestration class that ties all the service classes togethr to do what the program does
                await runner.RunAsync(); //run the orchestration class
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
