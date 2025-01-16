using HotelReservation.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public string ReservationRoomType { get; set; } = null!;

    public int ReservationRoomNumber { get; set; }

    public int? ReservationClientId { get; set; }

    public DateOnly ReservationIn { get; set; }

    public DateOnly ReservationOut { get; set; }

    public virtual Client? ReservationClient { get; set; } 

    public virtual Room ReservationRoomNumberNavigation { get; set; } = null!;
}
