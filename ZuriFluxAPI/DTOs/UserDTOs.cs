using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; }

    [RegularExpression("^(citizen|collector|admin)$",
        ErrorMessage = "Role must be citizen, collector, or admin.")]
    public string Role { get; set; } = "citizen";
}

public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
}

// What the API returns after successful login
public class AuthResponseDto
{
    public string Token { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

// What the API returns for user info
public class UserResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public int CreditBalance { get; set; }
    public DateTime CreatedAt { get; set; }
}

