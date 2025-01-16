using System;
using System.Collections.Generic;

namespace HotelReservation.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public DateOnly? DateRegister { get; set; }
}
