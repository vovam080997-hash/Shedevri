namespace CinemaGo.Models;


public abstract class Ticket
{
    public string TicketCode { get; }
    public decimal Price { get; protected set; }

    protected Ticket(string ticketCode)
    {
        TicketCode = ticketCode;
    }

  
    public abstract decimal CalculatePrice(decimal basePrice, decimal discountPercent);

    
    public abstract void PrintTicket(Booking booking, Movie movie, Session session, Hall hall);

    protected static decimal ApplyDiscount(decimal price, decimal discountPercent)
    {
        if (discountPercent < 0) discountPercent = 0;
        if (discountPercent > 100) discountPercent = 100;
        decimal result = price - (price * discountPercent / 100m);
        return result < 0 ? 0 : result; 
    }
}

public class StandardTicket : Ticket
{
    public StandardTicket(string ticketCode) : base(ticketCode) { }

    public override decimal CalculatePrice(decimal basePrice, decimal discountPercent)
    {
        Price = ApplyDiscount(basePrice, discountPercent);
        return Price;
    }

    public override void PrintTicket(Booking booking, Movie movie, Session session, Hall hall)
    {
        Console.WriteLine("------ Shedevri | Standard Ticket ------");
        Console.WriteLine($"Ticket code: {TicketCode}");
        Console.WriteLine($"Movie: {movie.Title} ({movie.AgeRating})");
        Console.WriteLine($"Hall: {hall.HallId} | Row {booking.Row}, Seat {booking.Seat}");
        Console.WriteLine($"Showtime: {session.StartDateTime:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Price: {Price} GEL");
        Console.WriteLine("-----------------------------------------");
    }
}


public class VipTicket : Ticket
{
    public VipTicket(string ticketCode) : base(ticketCode) { }

    public override decimal CalculatePrice(decimal basePrice, decimal discountPercent)
    {
        Price = ApplyDiscount(basePrice, discountPercent);
        return Price;
    }

    public override void PrintTicket(Booking booking, Movie movie, Session session, Hall hall)
    {
        Console.WriteLine("------ Shedevri | VIP Ticket ------");
        Console.WriteLine($"Ticket code: {TicketCode}");
        Console.WriteLine($"Movie: {movie.Title} ({movie.AgeRating})");
        Console.WriteLine($"Hall: {hall.HallId} | VIP Row {booking.Row}, Seat {booking.Seat}");
        Console.WriteLine($"Showtime: {session.StartDateTime:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Price: {Price} GEL");
        Console.WriteLine("------------------------------------");
    }
}
