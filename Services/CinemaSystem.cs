using System.Globalization;
using System.Text.RegularExpressions;
using CinemaGo.Models;

namespace CinemaGo.Services;

/// <summary>
/// CinemaSystem — orchestrates all business logic: registration/login, movies/sessions,
/// bookings/cancellations, statistics, export. Does not do file work itself (that's FileManager's job),
/// does not send emails itself (that's EmailService's job) — Single Responsibility principle.
/// </summary>
public class CinemaSystem
{
    private List<User> users;
    private List<Movie> movies;
    private List<Session> sessions;
    private List<Booking> bookings;
    private readonly List<Hall> halls;
    private readonly EmailService emailService;
    private readonly Dictionary<string, decimal> discountCodes;
    private readonly Random rng = new();

    public CinemaSystem()
    {
        users = FileManager.LoadUsers();
        movies = FileManager.LoadMovies();
        sessions = FileManager.LoadSessions();
        bookings = FileManager.LoadBookings();

        
        halls = new List<Hall>
        {
            new Hall("H1", rows: 6, seatsPerRow: 10, vipFromRow: 4), // 
            new Hall("H2", rows: 5, seatsPerRow: 8, vipFromRow: 4)
        };

    emailService = new EmailService(
 smtpHost: "smtp.gmail.com",
 smtpPort: 587,
 smtpUser: "vovam0809977@gmail.com",       
 smtpPassword: "smep piec conx qpap",       
 enabled: true); 

    discountCodes = new Dictionary<string, decimal>
        {
            { "STUDENT10", 10m },
            { "PROMO20", 20m }
        };

        UpdateSessionStatuses();
    }

   

    public void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Kidev erti shedevri chemgan");
        Console.WriteLine("=======================================");

        bool exit = false;
        while (!exit)
        {
            UpdateSessionStatuses();

            Console.WriteLine();
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Log in");
            Console.WriteLine("3. Exit program");
            Console.Write("Choose an option: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": Register(); break;
                case "2": Login(); break;
                case "3": exit = true; break;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
        Console.WriteLine("Program closed. Goodbye!");
    }

    private void Register()
    {
        Console.WriteLine("--- Registration ---");
        Console.Write("Username: ");
        string username = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(username) || users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Error: username is empty or already taken.");
            return;
        }

        Console.Write("Email: ");
        string email = Console.ReadLine()?.Trim() ?? "";
        if (!IsValidEmail(email) || users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Error: invalid email format or already in use.");
            return;
        }

        Console.WriteLine(PasswordRules);
        Console.Write("Password: ");
        string password = ReadPassword();
        if (!IsValidPassword(password))
        {
            Console.WriteLine("Error: password does not meet the requirements above.");
            return;
        }

        int newId = users.Count == 0 ? 1001 : users.Max(u => u.Id) + 1;
        var customer = new Customer(newId, username, password, email);
        users.Add(customer);
        FileManager.SaveUsers(users);
        Logger.Log($"New user registered: {username} (ID {newId})");
        Console.WriteLine($"Registration successful! Your ID: {newId}");
    }

    private void Login()
    {
        Console.WriteLine("--- Login ---");
        Console.Write("Username: ");
        string username = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Password: ");
        string password = ReadPassword();

        var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user == null || !user.CheckPassword(password))
        {
            Logger.Log($"Failed login attempt: {username}");
            Console.WriteLine("Incorrect username or password.");
            return;
        }

        Logger.Log($"Login successful: {user.Username} (role: {user.Role})");
        Console.WriteLine($"Welcome, {user.Username}!");
        user.DisplayMenu(this); 
    }


    public void ChangePassword(User user)
    {
        Console.WriteLine("--- Change password ---");
        Console.Write("Current password: ");
        string oldPassword = ReadPassword();
        if (!user.CheckPassword(oldPassword))
        {
            Console.WriteLine("Incorrect current password.");
            return;
        }

        Console.WriteLine(PasswordRules);
        Console.Write("New password: ");
        string newPassword = ReadPassword();
        if (!IsValidPassword(newPassword))
        {
            Console.WriteLine("Error: password does not meet the requirements above.");
            return;
        }

        Console.Write("Confirm new password: ");
        string confirmPassword = ReadPassword();
        if (newPassword != confirmPassword)
        {
            Console.WriteLine("Error: passwords do not match.");
            return;
        }

        if (!user.TryChangePassword(oldPassword, newPassword))
        {
            Console.WriteLine("Failed to change the password.");
            return;
        }

        FileManager.SaveUsers(users);
        Logger.Log($"User {user.Username} changed their password.");
        Console.WriteLine("Password changed successfully.");
    }

    private static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

  
    private static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} (yes/no): ");
        string answer = Console.ReadLine()?.Trim().ToLower() ?? "";
        return answer is "yes" or "y";
    }


    private static string ReadPassword()
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? "";

        var password = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return password.ToString();
    }

    
    private static bool IsValidPassword(string pw) =>
        !string.IsNullOrEmpty(pw)
        && pw.Length >= 8 && pw.Length <= 20
        && pw.Any(char.IsUpper)
        && pw.Any(c => !char.IsLetterOrDigit(c));

    private const string PasswordRules =
        "Password requirements: 8-20 characters, at least one uppercase letter (A-Z) and one special character (e.g. !@#$%^&*).";


    private void UpdateSessionStatuses()
    {
        bool changed = false;
        foreach (var session in sessions.Where(s => s.Status == SessionStatus.Active))
        {
            var movie = movies.FirstOrDefault(m => m.MovieId == session.MovieId);
            int duration = movie?.DurationMinutes ?? 120;
            if (session.StartDateTime.AddMinutes(duration) < DateTime.Now)
            {
                session.Status = SessionStatus.Completed;
                changed = true;
                Logger.Log($"Session {session.SessionId} automatically marked as completed.");
            }
        }
        if (changed) FileManager.SaveSessions(sessions);
    }

    private Hall? GetHall(string hallId) => halls.FirstOrDefault(h => h.HallId == hallId);
    private Movie? GetMovie(string movieId) => movies.FirstOrDefault(m => m.MovieId == movieId);
    private Session? GetSession(string sessionId) => sessions.FirstOrDefault(s => s.SessionId == sessionId);

    
    private List<Seat> BuildSeatMap(Session session, Hall hall)
    {
        var seats = hall.CreateSeats();
        var activeBookings = bookings.Where(b => b.SessionId == session.SessionId && b.Status == BookingStatus.Confirmed);
        foreach (var b in activeBookings)
        {
            var seat = seats.FirstOrDefault(s => s.Row == b.Row && s.Number == b.Seat);
            if (seat != null && !seat.IsBooked) seat.Book();
        }
        return seats;
    }

    private string GenerateTicketCode()
    {
        string code;
        do
        {
            code = "CG-" + string.Concat(Enumerable.Range(0, 6)
                .Select(_ => "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[rng.Next(32)]));
        } while (bookings.Any(b => b.TicketCode == code));
        return code;
    }

    private string GenerateBookingId()
    {
        int max = bookings.Count == 0 ? 500 : bookings.Select(b => int.TryParse(b.BookingId.TrimStart('B'), out int n) ? n : 500).Max();
        return $"B{max + 1}";
    }

    

    public void Customer_BrowseMoviesAndSessions()
    {
        Console.WriteLine("--- Movies & sessions ---");
        Console.Write("Filter by? (genre/hall/date) [Enter = show all]: ");
        string choice = Console.ReadLine()?.Trim().ToLower() ?? "";

       
        var query = from s in sessions
                     join m in movies on s.MovieId equals m.MovieId
                     where s.Status == SessionStatus.Active
                     select new { Session = s, Movie = m };

        if (choice == "genre")
        {
            Console.Write("Genre: ");
            string genre = Console.ReadLine()?.Trim() ?? "";
            query = query.Where(x => x.Movie.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
        }
        else if (choice == "hall")
        {
            Console.Write("Hall ID: ");
            string hallId = Console.ReadLine()?.Trim() ?? "";
            query = query.Where(x => x.Session.HallId.Equals(hallId, StringComparison.OrdinalIgnoreCase));
        }
        else if (choice == "date")
        {
            Console.Write("Date (yyyy-MM-dd): ");
            if (DateTime.TryParse(Console.ReadLine()?.Trim(), out var date))
                query = query.Where(x => x.Session.StartDateTime.Date == date.Date);
        }

        var list = query.OrderBy(x => x.Session.StartDateTime).ToList();
        if (!list.Any())
        {
            Console.WriteLine("No active sessions found.");
            return;
        }

        foreach (var x in list)
        {
            var hall = GetHall(x.Session.HallId);
            int freeSeats = hall == null ? 0 : BuildSeatMap(x.Session, hall).Count(s => !s.IsBooked);
            Console.WriteLine($"{x.Movie} | {x.Session} | Free seats: {freeSeats}");
        }
    }

    public void Customer_ViewHall()
    {
        Console.Write("Session ID: ");
        string sessionId = Console.ReadLine()?.Trim() ?? "";
        var session = GetSession(sessionId);
        if (session == null) { Console.WriteLine("Session not found."); return; }
        var hall = GetHall(session.HallId);
        if (hall == null) { Console.WriteLine("Hall not found."); return; }

        var seatMap = BuildSeatMap(session, hall);
        hall.PrintMap(seatMap);
    }

    public void Customer_BookTicket(Customer customer)
    {
        Console.Write("Session ID: ");
        string sessionId = Console.ReadLine()?.Trim() ?? "";
        var session = GetSession(sessionId);
        if (session == null) { Console.WriteLine("Session not found."); return; }

  
        if (session.Status != SessionStatus.Active || session.StartDateTime < DateTime.Now)
        {
            Console.WriteLine("This session cannot be booked (it is in the past or not active).");
            return;
        }

        var hall = GetHall(session.HallId);
        if (hall == null) { Console.WriteLine("Hall not found."); return; }
        var movie = GetMovie(session.MovieId);
        if (movie == null) { Console.WriteLine("Movie not found."); return; }

        var seatMap = BuildSeatMap(session, hall);
        hall.PrintMap(seatMap);

        Console.Write("Row number: ");
        if (!int.TryParse(Console.ReadLine(), out int row) || row < 1 || row > hall.Rows)
        {
            Console.WriteLine("Invalid row number.");
            return;
        }
        Console.Write("Seat number: ");
        if (!int.TryParse(Console.ReadLine(), out int seatNum) || seatNum < 1 || seatNum > hall.SeatsPerRow)
        {
            Console.WriteLine("Invalid seat number.");
            return;
        }

        var seat = seatMap.First(s => s.Row == row && s.Number == seatNum);
        if (seat.IsBooked)
        {
            Console.WriteLine("This seat is already booked.");
            return;
        }

 
        bool isVip = seat.IsVip;
        decimal basePrice = isVip ? session.VipPrice : session.StandardPrice;


        Console.Write("Discount code, if you have one (Enter — skip): ");
        string code = Console.ReadLine()?.Trim().ToUpper() ?? "";
        decimal discount = 0;
        if (!string.IsNullOrEmpty(code))
        {
            if (discountCodes.TryGetValue(code, out decimal d))
            {
                discount = d;
                Console.WriteLine($"Code accepted: -{d}%");
            }
            else
            {
                Console.WriteLine("Invalid code — proceeding without a discount.");
            }
        }

        string ticketCode = GenerateTicketCode();
        Ticket ticket = isVip ? new VipTicket(ticketCode) : new StandardTicket(ticketCode);
        decimal finalPrice = ticket.CalculatePrice(basePrice, discount); 

        seat.Book();

        var booking = new Booking(
            GenerateBookingId(), customer.Id, session.SessionId, row, seatNum,
            isVip ? TicketTypeKind.VIP : TicketTypeKind.Standard,
            finalPrice, ticketCode, BookingStatus.Confirmed);

        bookings.Add(booking);
        FileManager.SaveBookings(bookings);
        Logger.Log($"New booking: user {customer.Username} (ID {customer.Id}), " +
                   $"session {session.SessionId}, seat [{row},{seatNum}], type {booking.TicketType}, price {finalPrice} GEL, code {ticketCode}");

        Console.WriteLine();
        ticket.PrintTicket(booking, movie, session, hall);

        emailService.SendBookingConfirmation(
            customer.Email,
            "CinemaGo - Booking confirmation",
            $"Hello, {customer.Username}!\n\n" +
            $"Your booking has been confirmed.\n" +
            $"Movie: {movie.Title}\nHall: {hall.HallId}\nSeat: Row {row}, Seat {seatNum}\n" +
            $"Showtime: {session.StartDateTime:yyyy-MM-dd HH:mm}\nPrice: {finalPrice} GEL\nTicket code: {ticketCode}\n\n" +
            $"Thank you for using CinemaGo!");
    }

    public void Customer_ViewMyBookings(Customer customer)
    {
        var own = bookings.Where(b => b.UserId == customer.Id).OrderByDescending(b => b.BookingId).ToList();
        if (!own.Any()) { Console.WriteLine("You have no bookings."); return; }

        foreach (var b in own)
        {
            var session = GetSession(b.SessionId);
            var movie = session != null ? GetMovie(session.MovieId) : null;
            string movieTitle = movie?.Title ?? "Unknown movie";
            string when = session != null ? session.StartDateTime.ToString("yyyy-MM-dd HH:mm") : "-";
            Console.WriteLine($"[{b.BookingId}] {movieTitle} | {when} | Row {b.Row}, Seat {b.Seat} | " +
                              $"{b.TicketType} | {b.Price} GEL | Code: {b.TicketCode} | Status: {b.Status}");
        }
    }

    public void Customer_CancelBooking(Customer customer)
    {
        Console.Write("Booking ID to cancel: ");
        string bookingId = Console.ReadLine()?.Trim() ?? "";
        var booking = bookings.FirstOrDefault(b => b.BookingId.Equals(bookingId, StringComparison.OrdinalIgnoreCase));

        if (booking == null || booking.UserId != customer.Id)
        {
            Console.WriteLine("No such booking found under your account.");
            return;
        }
        if (booking.Status != BookingStatus.Confirmed)
        {
            Console.WriteLine("This booking is already cancelled.");
            return;
        }

        booking.Cancel();
        FileManager.SaveBookings(bookings);
        Logger.Log($"Booking cancelled: {booking.BookingId} (user {customer.Username}) — seat released.");
        Console.WriteLine("Booking successfully cancelled. The seat is free again.");
    }



    public void Admin_ListUsers()
    {
        Console.WriteLine("--- User list ---");
 
        foreach (var u in users)
            Console.WriteLine($"[{u.Id}] {u.Username} | {u.Email} | Role: {u.Role}");
    }

    public void Admin_AddMovie()
    {
        Console.WriteLine("--- Add movie ---");
        Console.Write("MovieID (e.g. M03): ");
        string id = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(id) || movies.Any(m => m.MovieId.Equals(id, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Error: ID is empty or already taken.");
            return;
        }
        Console.Write("Title: ");
        string title = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Genre: ");
        string genre = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Duration (minutes): ");
        if (!int.TryParse(Console.ReadLine(), out int duration) || duration <= 0)
        {
            Console.WriteLine("Error: invalid duration.");
            return;
        }
        Console.Write("Age rating (e.g. 12+): ");
        string age = Console.ReadLine()?.Trim() ?? "";

        movies.Add(new Movie(id, title, genre, duration, age));
        FileManager.SaveMovies(movies);
        Logger.Log($"Admin added a new movie: {id} - {title}");
        Console.WriteLine("Movie added successfully.");
    }

    public void Admin_DeleteMovie()
    {
        Console.WriteLine("--- Delete movie ---");
        Console.Write("MovieID: ");
        string id = Console.ReadLine()?.Trim() ?? "";
        var movie = GetMovie(id);
        if (movie == null) { Console.WriteLine("Movie not found."); return; }

        var relatedSessions = sessions.Where(s => s.MovieId == id).ToList();
        if (relatedSessions.Any())
        {
            int bookingCount = bookings.Count(b => relatedSessions.Any(s => s.SessionId == b.SessionId)
                                                    && b.Status == BookingStatus.Confirmed);
            Console.WriteLine($"This movie has {relatedSessions.Count} session(s) (including {bookingCount} active booking(s)).");
            Console.WriteLine("Deleting the movie will also delete all its sessions and bookings.");
            if (!Confirm("Are you sure you want to continue?"))
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            foreach (var s in relatedSessions)
                bookings.RemoveAll(b => b.SessionId == s.SessionId);
            sessions.RemoveAll(s => s.MovieId == id);
            FileManager.SaveSessions(sessions);
            FileManager.SaveBookings(bookings);
        }

        movies.Remove(movie);
        FileManager.SaveMovies(movies);
        Logger.Log($"Admin deleted movie: {id} - {movie.Title} (together with its sessions and bookings).");
        Console.WriteLine("Movie deleted successfully.");
    }

    public void Admin_AddSession()
    {
        Console.WriteLine("--- Add session ---");
        Console.Write("SessionID (e.g. S102): ");
        string id = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(id) || sessions.Any(s => s.SessionId.Equals(id, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Error: ID is empty or already taken.");
            return;
        }
        Console.Write("MovieID: ");
        string movieId = Console.ReadLine()?.Trim() ?? "";
        if (GetMovie(movieId) == null) { Console.WriteLine("Error: no such movie exists."); return; }

        Console.Write("HallID (H1/H2): ");
        string hallId = Console.ReadLine()?.Trim() ?? "";
        if (GetHall(hallId) == null) { Console.WriteLine("Error: no such hall exists."); return; }

        Console.Write("Start date/time (yyyy-MM-dd HH:mm): ");
        if (!DateTime.TryParseExact(Console.ReadLine()?.Trim(), "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) || start < DateTime.Now)
        {
            Console.WriteLine("Error: invalid date or it is in the past.");
            return;
        }

        Console.Write("Standard price: ");
        if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out var stdPrice) || stdPrice < 0)
        {
            Console.WriteLine("Error: invalid price.");
            return;
        }
        Console.Write("VIP price: ");
        if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vipPrice) || vipPrice < 0)
        {
            Console.WriteLine("Error: invalid price.");
            return;
        }

        sessions.Add(new Session(id, movieId, hallId, start, stdPrice, vipPrice, SessionStatus.Active));
        FileManager.SaveSessions(sessions);
        Logger.Log($"Admin added a new session: {id} ({movieId}, {hallId}, {start:yyyy-MM-dd HH:mm})");
        Console.WriteLine("Session added successfully.");
    }

    public void Admin_DeleteSession()
    {
        Console.WriteLine("--- Delete session ---");
        Console.Write("SessionID: ");
        string id = Console.ReadLine()?.Trim() ?? "";
        var session = GetSession(id);
        if (session == null) { Console.WriteLine("Session not found."); return; }

        var activeBookings = bookings.Where(b => b.SessionId == id && b.Status == BookingStatus.Confirmed).ToList();
        if (activeBookings.Any())
        {
            Console.WriteLine($"This session has {activeBookings.Count} active booking(s).");
            Console.WriteLine("Deleting the session will automatically cancel these bookings.");
            if (!Confirm("Are you sure you want to continue?"))
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            foreach (var b in activeBookings)
            {
                b.Cancel();
                Logger.Log($"Booking {b.BookingId} automatically cancelled due to deletion of session {id}.");
            }
            FileManager.SaveBookings(bookings);
        }

        sessions.Remove(session);
        FileManager.SaveSessions(sessions);
        Logger.Log($"Admin deleted session: {id}.");
        Console.WriteLine("Session deleted successfully.");
    }

    public void Admin_EditSessionPrice()
    {
        Console.WriteLine("--- Edit session price ---");
        Console.Write("SessionID: ");
        string id = Console.ReadLine()?.Trim() ?? "";
        var session = GetSession(id);
        if (session == null) { Console.WriteLine("Session not found."); return; }

        Console.WriteLine($"Current prices — Standard: {session.StandardPrice} GEL | VIP: {session.VipPrice} GEL");

        Console.Write("New standard price (Enter — keep unchanged): ");
        string stdInput = Console.ReadLine()?.Trim() ?? "";
        decimal newStandard = session.StandardPrice;
        if (!string.IsNullOrEmpty(stdInput))
        {
            if (!decimal.TryParse(stdInput, NumberStyles.Any, CultureInfo.InvariantCulture, out newStandard) || newStandard < 0)
            {
                Console.WriteLine("Error: invalid price. Change cancelled.");
                return;
            }
        }

        Console.Write("New VIP price (Enter — keep unchanged): ");
        string vipInput = Console.ReadLine()?.Trim() ?? "";
        decimal newVip = session.VipPrice;
        if (!string.IsNullOrEmpty(vipInput))
        {
            if (!decimal.TryParse(vipInput, NumberStyles.Any, CultureInfo.InvariantCulture, out newVip) || newVip < 0)
            {
                Console.WriteLine("Error: invalid price. Change cancelled.");
                return;
            }
        }

        session.StandardPrice = newStandard;
        session.VipPrice = newVip;
        FileManager.SaveSessions(sessions);
        Logger.Log($"Admin changed prices for session {id} — Standard: {newStandard} GEL, VIP: {newVip} GEL.");
        Console.WriteLine("Price updated successfully. Note: already issued tickets keep their original price.");
    }

    public void Admin_BrowseMoviesAndSessions() => Customer_BrowseMoviesAndSessions();

    public void Admin_ShowStatistics()
    {
        Console.WriteLine("--- Statistics (LINQ) ---");


        var popular = bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Join(sessions, b => b.SessionId, s => s.SessionId, (b, s) => s.MovieId)
            .GroupBy(movieId => movieId)
            .Select(g => new { MovieId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        Console.WriteLine("Most popular movies (by number of bookings):");
        if (!popular.Any()) Console.WriteLine("  No bookings exist yet.");
        foreach (var p in popular)
        {
            var movie = GetMovie(p.MovieId);
            Console.WriteLine($"  {movie?.Title ?? p.MovieId}: {p.Count} booking(s)");
        }


        Console.WriteLine();
        Console.WriteLine("Free seats on active sessions:");
        foreach (var session in sessions.Where(s => s.Status == SessionStatus.Active))
        {
            var hall = GetHall(session.HallId);
            if (hall == null) continue;
            int free = BuildSeatMap(session, hall).Count(s => !s.IsBooked);
            Console.WriteLine($"  [{session.SessionId}] {free} / {hall.TotalSeats} free");
        }

  
        Console.WriteLine();
        Console.WriteLine("Number of sessions by genre:");
        var byGenre = sessions
            .Join(movies, s => s.MovieId, m => m.MovieId, (s, m) => m.Genre)
            .GroupBy(g => g)
            .Select(g => new { Genre = g.Key, Count = g.Count() });
        foreach (var g in byGenre)
            Console.WriteLine($"  {g.Genre}: {g.Count} session(s)");
    }

    public void Admin_ExportData()
    {
        Console.WriteLine("--- Export data to CSV ---");

        FileManager.ExportToCsv("Users_export.csv", users,
            "ID,Username,Email,Role",
            u => $"{u.Id},{u.Username},{u.Email},{u.Role}");

        FileManager.ExportToCsv("Movies_export.csv", movies,
            "MovieID,Title,Genre,DurationMinutes,AgeRating",
            m => $"{m.MovieId},{m.Title},{m.Genre},{m.DurationMinutes},{m.AgeRating}");

        FileManager.ExportToCsv("Sessions_export.csv", sessions,
            "SessionID,MovieID,HallID,StartDateTime,StandardPrice,VipPrice,Status",
            s => $"{s.SessionId},{s.MovieId},{s.HallId},{s.StartDateTime:yyyy-MM-dd HH:mm},{s.StandardPrice},{s.VipPrice},{s.Status}");

        FileManager.ExportToCsv("Bookings_export.csv", bookings,
            "BookingID,UserID,SessionID,Row,Seat,TicketType,Price,TicketCode,Status",
            b => $"{b.BookingId},{b.UserId},{b.SessionId},{b.Row},{b.Seat},{b.TicketType},{b.Price},{b.TicketCode},{b.Status}");

        Logger.Log("Admin performed a full CSV data export.");
    }

    public void Admin_ShowLogs()
    {
        Console.WriteLine("--- Recent logs ---");
        var logs = Logger.ReadAll();
        foreach (var line in logs.TakeLast(30))
            Console.WriteLine(line);
        if (!logs.Any()) Console.WriteLine("No logs yet.");
    }
}
