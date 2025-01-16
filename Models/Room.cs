using System;
using System.Collections.Generic;

namespace HotelReservation.Models;

public partial class Room
{
    public int RoomNumber { get; set; }

    public string RoomType { get; set; } = null!;

    public string RoomPhone { get; set; } = null!;

    public string RoomFree { get; set; } = null!;

    public int RoomCapacity { get; set; }

    public decimal RoomPrice { get; set; }

    public string RoomStatus { get; set; } = null!;
    public byte[]? Room_Image { get; set; }

    public DateOnly DateCreated { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
