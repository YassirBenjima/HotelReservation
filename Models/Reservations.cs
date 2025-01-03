using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class Reservation
    {
        [Key]
        [Column("Reservation_ID")]
        public int ReservationId { get; set; }

        [Required]
        [StringLength(6)]
        [Column("Reservation_Room_Type")]
        public string RoomType { get; set; }

        [Required]
        [Column("Reservation_Room_Number")]
        public int RoomNumber { get; set; }

        [Required]
        [Column("Reservation_Client_ID")]
        public string ClientId { get; set; }

        [Required]
        [Column("Reservation_In")]
        public DateTime CheckIn { get; set; }

        [Required]
        [Column("Reservation_Out")]
        public DateTime CheckOut { get; set; }

        [ForeignKey("ClientId")]
        public Client Client { get; set; }
    }
}
