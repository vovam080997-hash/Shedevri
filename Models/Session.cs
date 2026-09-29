using System.Globalization;

namespace CinemaGo.Models;

public enum SessionStatus
{
    Active,
    Completed,
    Cancelled
}


public class Session
{
    public string SessionId { get; set; }
    public string MovieId { get; set; }
    public string HallId { get; set; }
    public DateTime StartDateTime { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal VipPrice { get; set; }
    public SessionStatus Status { get; set; }

    private const string DateFormat = "yyyy-MM-dd HH:mm";

    public Session(string sessionId, string movieId, string hallId, DateTime startDateTime,
        decimal standardPrice, decimal vipPrice, SessionStatus status)
    {
        SessionId = sessionId;
        MovieId = movieId;
        HallId = hallId;
        StartDateTime = startDateTime;
        StandardPrice = standardPrice;
        VipPrice = vipPrice;
        Status = status;
    }

    public string ToFileLine() =>
        $"{SessionId}|{MovieId}|{HallId}|{StartDateTime.ToString(DateFormat)}|{StandardPrice}|{VipPrice}|{Status}";

    public static Session? FromFileLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 7) return null;
        if (!DateTime.TryParseExact(p[3].Trim(), DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var start))
            return null;
        if (!decimal.TryParse(p[4].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var stdPrice))
            return null;
        if (!decimal.TryParse(p[5].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vipPrice))
            return null;
        if (!Enum.TryParse<SessionStatus>(p[6].Trim(), true, out var status))
            status = SessionStatus.Active;

        return new Session(p[0].Trim(), p[1].Trim(), p[2].Trim(), start, stdPrice, vipPrice, status);
    }

    public override string ToString() =>
        $"[{SessionId}] Hall: {HallId} | Time: {StartDateTime:yyyy-MM-dd HH:mm} | " +
        $"Standard: {StandardPrice} GEL | VIP: {VipPrice} GEL | Status: {Status}";
}
