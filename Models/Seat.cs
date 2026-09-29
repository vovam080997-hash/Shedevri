namespace CinemaGo.Models;


public class Seat
{
    public int Row { get; }
    public int Number { get; }
    public bool IsVip { get; }
    private bool isBooked;

    public bool IsBooked => isBooked;

    public Seat(int row, int number, bool isVip, bool isBooked = false)
    {
        Row = row;
        Number = number;
        IsVip = isVip;
        this.isBooked = isBooked;
    }

    
    public void Book()
    {
        if (isBooked)
            throw new InvalidOperationException("This seat is already booked.");
        isBooked = true;
    }

  
    public void Cancel()
    {
        isBooked = false;
    }

  
    public char StatusSymbol => isBooked ? 'X' : (IsVip ? 'V' : 'O');
}
