using System.ComponentModel.DataAnnotations;

namespace BasketballTournamentsApp.Models
{
    public class Team
    {
        public int TeamId {  get; set; }

        [Required(ErrorMessage = "The team's name is required!")]
        [StringLength(100, ErrorMessage ="The team's name cannot exceed 100 characters!")]
        public string TeamName {  get; set; }

        [Required(ErrorMessage ="CreatedByUserId is required!")]
        public int CreatedByUserId {  get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Url(ErrorMessage = "The URL format is invalid!")]
        [StringLength(255, ErrorMessage ="The logo URL cannot exceed 255 characters!")]
        public string LogoUrl {  get; set; }
    }
}
