using CinemaGo.Services;

namespace CinemaGo.Models;


public class Admin : User
{
    public Admin(int id, string username, string password, string email)
        : base(id, username, password, email, "admin") { }

    public override void DisplayMenu(CinemaSystem system)
    {
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine();
            Console.WriteLine("========== ADMIN MENU ==========");
            Console.WriteLine("1. List users");
            Console.WriteLine("2. Add movie");
            Console.WriteLine("3. Delete movie");
            Console.WriteLine("4. Add session");
            Console.WriteLine("5. Delete session");
            Console.WriteLine("6. Edit session price");
            Console.WriteLine("7. Browse movies & sessions (list/filter)");
            Console.WriteLine("8. Statistics (LINQ)");
            Console.WriteLine("9. Export data (CSV)");
            Console.WriteLine("10. View logs");
            Console.WriteLine("11. Change password");
            Console.WriteLine("12. Log out");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": system.Admin_ListUsers(); break;
                case "2": system.Admin_AddMovie(); break;
                case "3": system.Admin_DeleteMovie(); break;
                case "4": system.Admin_AddSession(); break;
                case "5": system.Admin_DeleteSession(); break;
                case "6": system.Admin_EditSessionPrice(); break;
                case "7": system.Admin_BrowseMoviesAndSessions(); break;
                case "8": system.Admin_ShowStatistics(); break;
                case "9": system.Admin_ExportData(); break;
                case "10": system.Admin_ShowLogs(); break;
                case "11": system.ChangePassword(this); break;
                case "12": exit = true; break;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }
}
