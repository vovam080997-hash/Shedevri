namespace CinemaGo.Services;


public static class Logger
{
    private static readonly string LogPath = Path.Combine(ResolveDataDir(), "log.txt");

    
    private static string ResolveDataDir()
    {
        try
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (dir.GetFiles("*.csproj").Length > 0)
                    return Path.Combine(dir.FullName, "Data");
                dir = dir.Parent;
            }
        }
        catch
        {
            
        }
        return Path.Combine(AppContext.BaseDirectory, "Data");
    }

    public static void Log(string action)
    {
        try
        {
            var dir = Path.GetDirectoryName(LogPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}";
            File.AppendAllLines(LogPath, new[] { line });
        }
        catch
        {
            
        }
    }

    public static List<string> ReadAll()
    {
        try
        {
            return File.Exists(LogPath) ? File.ReadAllLines(LogPath).ToList() : new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
