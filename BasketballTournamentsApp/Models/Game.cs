using System.ComponentModel.DataAnnotations;

namespace BasketballTournamentsApp.Models
{
    public class Game
    {
        public int GameId { get; set; }

        [Required(ErrorMessage ="TournamentId is required!")]
        public int TournamentId {  get; set; }

        [Required(ErrorMessage = "The game's round number is required!")]
        [StringLength(50, ErrorMessage = "The game's round number cannot exceed 50 characters!")]
        public string RoundNumber {  get; set; }

        [Required(ErrorMessage="The game's time is required!")]
        [DataType(DataType.DateTime)]
        public DateTime GameTime { get; set; }

        [Required(ErrorMessage = "The first team's ID is required!")]
        public int Team1Id {  get; set; }

        [Required(ErrorMessage = "The second team's ID is required!")]
        public int Team2Id {  get; set; }

        public int? ScoreTeam1 {  get; set; }

        public int? ScoreTeam2 {  get; set; }

        [Required(ErrorMessage ="The game's status is required!")]
        [RegularExpression("Scheduled|InProgress|Completed|Cancelled", 
            ErrorMessage = "GameStatus must be one of: Scheduled, InProgress, Completed or Cancelled.")]
        public string GameStatus {  get; set; }


    }
}
