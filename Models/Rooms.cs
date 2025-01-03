using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class Room
    {
        [Key]
        [Column("Room_Number")]
        public int RoomNumber { get; set; }

        [Required]
        [StringLength(6)]
        [Column("Room_Type")]
        public string RoomType { get; set; }

        [StringLength(15)]
        [Column("Room_Phone")]
        public string RoomPhone { get; set; }

        [Required]
        [StringLength(3)]
        [Column("Room_Free")]
        public string IsFree { get; set; }

        [Required]
        [Column("Room_Capacity")]
        public int Capacity { get; set; }

        [Required]
        [Column("Room_Price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(10)]
        [Column("Room_Status")]
        public string Status { get; set; }

        [Required]
        [Column("Date_Created")]
        public DateTime DateCreated { get; set; }
    }
}
