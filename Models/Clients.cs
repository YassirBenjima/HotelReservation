using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; 
namespace HotelReservation.Models
{
    public class Client : IdentityUser
    {
        [Key]
        [Column("id_client")]
        public int IdClient { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        [Column("FirstName")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Column("LastName")]
        public string LastName { get; set; }

        [StringLength(15)]
        public string Phone { get; set; }

        [Column(TypeName = "text")]
        public string Address { get; set; }
    }
}
