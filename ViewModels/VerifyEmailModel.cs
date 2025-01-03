﻿using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class VerifyEmailModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
