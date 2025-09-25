using System.ComponentModel.DataAnnotations;

namespace BasketballTournamentsApp.Models
{
    public class User
    {
        public int UserId {  get; set; }

        [Required(ErrorMessage="Username is required!")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters!")]
        public string Username {  get; set; }

        [Required(ErrorMessage = "First name is required!")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters!")]
        public string FirstName {  get; set; }

        [Required(ErrorMessage = "Last name is required!")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters!")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Invalid email format!")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters!")]
        public string Email { get; set; }

        [Phone]
        [StringLength(10, ErrorMessage = "Invalid phone number!")]
        public string PhoneNumber {  get; set; }

        [StringLength(500, ErrorMessage = "Refresh token cannot exceed 500 characters!")]
        public string RefreshToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Role is required!")]
        [RegularExpression("Player|Admin|Referee",
           ErrorMessage = "Role must be one of: Player, Admin or Referee.")]
        public string PersonRole { get; set; } = "Player";

        public bool IsEmailVerified { get; set; } = false;

        public DateTime? RefreshTokenExpiry { get; set; }


    }
}
