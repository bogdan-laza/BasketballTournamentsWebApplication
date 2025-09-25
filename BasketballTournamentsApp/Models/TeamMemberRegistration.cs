using System.ComponentModel.DataAnnotations;

namespace BasketballTournamentsApp.Models
{
    public class TeamMemberRegistration
    {
        public int UserId {  get; set; }

        public int TeamId {  get; set; }

        [DataType(DataType.DateTime)]
        public DateTime JoinedAt { get; set; }
    }
}
