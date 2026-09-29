using CinemaGo.Models;

namespace CinemaGo.Services;


public static class FileManager
{
    private static readonly string DataDir = ResolveDataDir();


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

    private static string PathOf(string fileName) => Path.Combine(DataDir, fileName);

    private static List<string> ReadLinesSafe(string fileName)
    {
        try
        {
            var path = PathOf(fileName);
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
                Logger.Log($"File {fileName} not found — created a new empty file.");
                return new List<string>();
            }
            return File.ReadAllLines(path)
                       .Where(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("//"))
                       .ToList();
        }
        catch (Exception ex)
        {
            Logger.Log($"File read error ({fileName}): {ex.Message}");
            return new List<string>();
        }
    }

    private static void WriteLinesSafe(string fileName, IEnumerable<string> lines, string header)
    {
        try
        {
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
            var content = new List<string> { header };
            content.AddRange(lines);
            File.WriteAllLines(PathOf(fileName), content);
        }
        catch (Exception ex)
        {
            Logger.Log($"File write error ({fileName}): {ex.Message}");
            Console.WriteLine("Warning: failed to save data (see log.txt).");
        }
    }

 
    public static List<User> LoadUsers()
    {
        var result = new List<User>();
        foreach (var line in ReadLinesSafe("Users.txt"))
        {
            var u = User.FromFileLine(line);
            if (u != null) result.Add(u);
            else Logger.Log($"Corrupted record in Users.txt skipped: {line}");
        }
        return result;
    }

    public static void SaveUsers(List<User> users) =>
        WriteLinesSafe("Users.txt", users.Select(u => u.ToFileLine()),
            "// ID | Username | Password | Email | Role");

 
    public static List<Movie> LoadMovies()
    {
        var result = new List<Movie>();
        foreach (var line in ReadLinesSafe("Movies.txt"))
        {
            var m = Movie.FromFileLine(line);
            if (m != null) result.Add(m);
            else Logger.Log($"Corrupted record in Movies.txt skipped: {line}");
        }
        return result;
    }

    public static void SaveMovies(List<Movie> movies) =>
        WriteLinesSafe("Movies.txt", movies.Select(m => m.ToFileLine()),
            "// MovieID | Title | Genre | DurationMinutes | AgeRating");

   
    public static List<Session> LoadSessions()
    {
        var result = new List<Session>();
        foreach (var line in ReadLinesSafe("Sessions.txt"))
        {
            var s = Session.FromFileLine(line);
            if (s != null) result.Add(s);
            else Logger.Log($"Corrupted record in Sessions.txt skipped: {line}");
        }
        return result;
    }

    public static void SaveSessions(List<Session> sessions) =>
        WriteLinesSafe("Sessions.txt", sessions.Select(s => s.ToFileLine()),
            "// SessionID | MovieID | HallID | StartDateTime | StandardPrice | VipPrice | Status");

   
    public static List<Booking> LoadBookings()
    {
        var result = new List<Booking>();
        foreach (var line in ReadLinesSafe("Bookings.txt"))
        {
            var b = Booking.FromFileLine(line);
            if (b != null) result.Add(b);
            else Logger.Log($"Corrupted record in Bookings.txt skipped: {line}");
        }
        return result;
    }

    public static void SaveBookings(List<Booking> bookings) =>
        WriteLinesSafe("Bookings.txt", bookings.Select(b => b.ToFileLine()),
            "// BookingID | UserID | SessionID | Row | Seat | TicketType | Price | TicketCode | Status");

   
    public static void ExportToCsv<T>(string fileName, IEnumerable<T> items, string header, Func<T, string> toCsvLine)
    {
        try
        {
            var lines = new List<string> { header };
            lines.AddRange(items.Select(toCsvLine));
            File.WriteAllLines(PathOf(fileName), lines);
            Logger.Log($"CSV export completed: {fileName}");
            Console.WriteLine($"Export completed: Data/{fileName}");
        }
        catch (Exception ex)
        {
            Logger.Log($"CSV export error ({fileName}): {ex.Message}");
            Console.WriteLine("Warning: export failed (see log.txt).");
        }
    }
}
