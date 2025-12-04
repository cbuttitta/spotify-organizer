namespace SpotifyOrganizer.Core
{
    public static class ConsolePrompts
    {
        public static bool AskYesNo(string prompt, bool defaultAnswer = false)
        {
            Console.Write(prompt + (defaultAnswer ? " (Y/n): " : " (y/N): "));
            string ans = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(ans)) return defaultAnswer;
            return ans.Trim().ToLower() switch
            {
                "y" or "yes" => true,
                "n" or "no" => false,
                _ => defaultAnswer
            };
        }

        public static string AskLine(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? string.Empty;
        }
    }
}
