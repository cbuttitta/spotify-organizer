namespace SpotifyOrganizer.Utils
{
    public static class ProgressBar
    {
        public static void Draw(int progress, int total, int barSize = 40)
        {
            if (total <= 0) return;
            double percent = (double)progress / total;
            int filled = (int)(percent * barSize);
            string bar = new string('█', filled) + new string(' ', barSize - filled);
            Console.CursorLeft = 0;
            Console.Write($"[{bar}] {progress}/{total} ({percent:P0})");
        }
    }
}
