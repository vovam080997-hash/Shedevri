using System.Globalization;

namespace CinemaGo.Models;

public enum BookingStatus
{
    Confirmed,
    Cancelled
}

public enum TicketTypeKind
{
    Standard,
    VIP
}


public class Booking
{
    public string BookingId { get; set; }
    public int UserId { get; set; }
    public string SessionId { get; set; }
    public int Row { get; set; }
    public int Seat { get; set; }
    public TicketTypeKind TicketType { get; set; }
    public decimal Price { get; set; }
    public string TicketCode { get; set; }
    public BookingStatus Status { get; private set; }

    public Booking(string bookingId, int userId, string sessionId, int row, int seat,
        TicketTypeKind ticketType, decimal price, string ticketCode, BookingStatus status)
    {
        BookingId = bookingId;
        UserId = userId;
        SessionId = sessionId;
        Row = row;
        Seat = seat;
        TicketType = ticketType;
        Price = price < 0 ? 0 : price; 
        TicketCode = ticketCode;
        Status = status;
    }

   
    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("This booking is already cancelled.");
        Status = BookingStatus.Cancelled;
    }

    public string ToFileLine() =>
        $"{BookingId}|{UserId}|{SessionId}|{Row}|{Seat}|{TicketType}|{Price.ToString(CultureInfo.InvariantCulture)}|{TicketCode}|{Status}";

    public static Booking? FromFileLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 9) return null;
        if (!int.TryParse(p[1].Trim(), out int userId)) return null;
        if (!int.TryParse(p[3].Trim(), out int row)) return null;
        if (!int.TryParse(p[4].Trim(), out int seat)) return null;
        if (!Enum.TryParse<TicketTypeKind>(p[5].Trim(), true, out var type)) return null;
        if (!decimal.TryParse(p[6].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) return null;
        if (!Enum.TryParse<BookingStatus>(p[8].Trim(), true, out var status)) status = BookingStatus.Confirmed;

        return new Booking(p[0].Trim(), userId, p[2].Trim(), row, seat, type, price, p[7].Trim(), status);
    }
}
