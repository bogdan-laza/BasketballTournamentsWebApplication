using System.ComponentModel.DataAnnotations;

namespace BasketballTournamentsApp.Models
{
    public class TeamRegistration
    {
        public int TournamentId {  get; set; }

        public int TeamId {  get; set; }

        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; }
    }
}
