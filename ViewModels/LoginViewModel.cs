using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'adresse e-mail est requise.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
    