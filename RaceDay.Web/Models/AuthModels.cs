namespace RaceDay.Web.Models;

public class LoginModel
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Participant";

    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }
}

public class AuthResponseModel
{
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}