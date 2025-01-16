using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.ViewModels
{
    public class ConfirmationViewModel
    {
        public int ReservationId { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string ClientName { get; set; }
        public decimal TotalAmount { get; set; }
        public byte[] Room_Image { get; set; } // Pour stocker les données d'image

    }


}
