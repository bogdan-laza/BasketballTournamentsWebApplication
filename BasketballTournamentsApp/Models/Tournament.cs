using System.ComponentModel.DataAnnotations;

namespace BasketballTournamentsApp.Models
{
    public class Tournament
    {
        public int TournamentId {  get; set; }

        [Required(ErrorMessage ="Tournament name is required!")]
        [StringLength(100, ErrorMessage ="Tournament name cannot exceed 100 characters!")]
        public string TournamentName {get; set; }

        [Required(ErrorMessage = "Tournament date is required!")]
        public DateTime TournamentDate { get; set; }

        [Required(ErrorMessage = "Tournament location is required!")]
        [StringLength(150, ErrorMessage = "Tournament location cannot exceed 150 characters!")]
        public string TournamentLocation {  get; set; }

        [Required(ErrorMessage = "Tournament entry fee is required!")]
        [Range(0, 9999999.99, ErrorMessage = "Tournament entry fee must be between 0 and 9,999,999.99.")]
        public decimal EntryFee {  get; set; }

        [Required(ErrorMessage = "Tournament prize is required!")]
        [Range(0, 9999999.99, ErrorMessage = "Tournament prize must be between 0 and 9,999,999.99.")]
        public decimal Prize {  get; set; }
        public string? Rules {  get; set; }

        [Required(ErrorMessage = "Tournament format is required.")]
        [StringLength(50, ErrorMessage = "Tournament format cannot exceed 50 characters.")]
        public string TournamentFormat {  get; set; }

        [Required(ErrorMessage = "The user id is required.")]
        public int CreatedByUserId {  get; set; }

        [Range(2, 256, ErrorMessage = "Number of teams must be between 2 and 256.")]
        public int MaximumNumberOfTeams {  get; set; }

        [Required(ErrorMessage ="GameStatus is required!")]
        [RegularExpression("Upcoming|Ongoing|Completed|Cancelled",
           ErrorMessage = "GameStatus must be one of: Upcoming, Ongoing, Completed, Cancelled.")]
        public string Status {  get; set; }

        [Required(ErrorMessage = "Registration deadline is required.")]
        public DateTime RegistrationDeadline { get; set; }

        [Url(ErrorMessage = "The URL format is invalid!")]
        [StringLength(255, ErrorMessage ="The URL cannot exceed 255 characters!")]
        public string TournamentImageUrl { get; set; }
    }
}
