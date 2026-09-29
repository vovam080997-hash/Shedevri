using CinemaGo.Services;

namespace CinemaGo.Models;


public abstract class User
{
    public int Id { get; protected set; }
    public string Username { get; protected set; }
    public string Email { get; protected set; }
    public string Role { get; protected set; }

  
    private string password;

    protected User(int id, string username, string password, string email, string role)
    {
        Id = id;
        Username = username;
        this.password = password;
        Email = email;
        Role = role;
    }

  
    public bool CheckPassword(string inputPassword) => password == inputPassword;

    protected void SetPassword(string newPassword) => password = newPassword;


    public bool TryChangePassword(string oldPassword, string newPassword)
    {
        if (!CheckPassword(oldPassword)) return false;
        SetPassword(newPassword);
        return true;
    }

    public abstract void DisplayMenu(CinemaSystem system);

   
    public string ToFileLine() => $"{Id}|{Username}|{password}|{Email}|{Role}";

    public static User? FromFileLine(string line)
    {
        var parts = line.Split('|');
        if (parts.Length < 5) return null;

        if (!int.TryParse(parts[0].Trim(), out int id)) return null;
        string username = parts[1].Trim();
        string pass = parts[2].Trim();
        string email = parts[3].Trim();
        string role = parts[4].Trim().ToLower();

        return role switch
        {
            "admin" => new Admin(id, username, pass, email),
            "customer" => new Customer(id, username, pass, email),
            _ => null
        };
    }
}
