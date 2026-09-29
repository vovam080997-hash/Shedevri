namespace CinemaGo.Models;


public class Hall
{
    public string HallId { get; }
    public int Rows { get; }
    public int SeatsPerRow { get; }
    public int VipFromRow { get; } 

    public Hall(string hallId, int rows, int seatsPerRow, int vipFromRow)
    {
        HallId = hallId;
        Rows = rows;
        SeatsPerRow = seatsPerRow;
        VipFromRow = vipFromRow;
    }

    public bool IsVipRow(int row) => row >= VipFromRow;

    public int TotalSeats => Rows * SeatsPerRow;


    public List<Seat> CreateSeats()
    {
        var seats = new List<Seat>();
        for (int r = 1; r <= Rows; r++)
            for (int s = 1; s <= SeatsPerRow; s++)
                seats.Add(new Seat(r, s, IsVipRow(r)));
        return seats;
    }

    public void PrintMap(List<Seat> seats)
    {
        Console.WriteLine($"Hall {HallId} — Free [O], Booked [X], VIP [V]");
        Console.Write("       ");
        for (int s = 1; s <= SeatsPerRow; s++) Console.Write($"{s,3}");
        Console.WriteLine();

        for (int r = 1; r <= Rows; r++)
        {
            Console.Write($"Row {r,2}: ");
            for (int s = 1; s <= SeatsPerRow; s++)
            {
                var seat = seats.First(x => x.Row == r && x.Number == s);
                Console.Write($"[{seat.StatusSymbol}]");
            }
            Console.WriteLine();
        }
    }
}
