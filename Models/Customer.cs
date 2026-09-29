using CinemaGo.Services;

namespace CinemaGo.Models;


public class Customer : User
{
    public Customer(int id, string username, string password, string email)
        : base(id, username, password, email, "customer") { }

    public override void DisplayMenu(CinemaSystem system)
    {
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine();
            Console.WriteLine($"========== CUSTOMER MENU ({Username}) ==========");
            Console.WriteLine("1. Browse movies & sessions (filter)");
            Console.WriteLine("2. View hall seating");
            Console.WriteLine("3. Book a ticket");
            Console.WriteLine("4. My bookings");
            Console.WriteLine("5. Cancel a booking");
            Console.WriteLine("6. Change password");
            Console.WriteLine("7. Log out");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": system.Customer_BrowseMoviesAndSessions(); break;
                case "2": system.Customer_ViewHall(); break;
                case "3": system.Customer_BookTicket(this); break;
                case "4": system.Customer_ViewMyBookings(this); break;
                case "5": system.Customer_CancelBooking(this); break;
                case "6": system.ChangePassword(this); break;
                case "7": exit = true; break;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }
}
