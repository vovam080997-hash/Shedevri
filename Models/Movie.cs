namespace CinemaGo.Models;


public class Movie
{
    public string MovieId { get; set; }
    public string Title { get; set; }
    public string Genre { get; set; }
    public int DurationMinutes { get; set; }
    public string AgeRating { get; set; }

    public Movie(string movieId, string title, string genre, int durationMinutes, string ageRating)
    {
        MovieId = movieId;
        Title = title;
        Genre = genre;
        DurationMinutes = durationMinutes;
        AgeRating = ageRating;
    }

    public string ToFileLine() => $"{MovieId}|{Title}|{Genre}|{DurationMinutes}|{AgeRating}";

    public static Movie? FromFileLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 5) return null;
        if (!int.TryParse(p[3].Trim(), out int duration)) return null;
        return new Movie(p[0].Trim(), p[1].Trim(), p[2].Trim(), duration, p[4].Trim());
    }

    public override string ToString() =>
        $"[{MovieId}] {Title} | Genre: {Genre} | Duration: {DurationMinutes} min | Age rating: {AgeRating}";
}
