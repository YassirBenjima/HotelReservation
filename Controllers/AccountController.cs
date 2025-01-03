using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.ViewModels;

namespace HotelReservation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<Client> _signInManager;
        private readonly UserManager<Client> _userManager;

        public AccountController(ApplicationDbContext context, SignInManager<Client> signInManager, UserManager<Client> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public IActionResult Login()
        {
            var roomTypes = _context.Rooms
                .Where(r => r.Status == "Actif")
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            var rooms = _context.Rooms
                .Where(r => r.Status == "Actif")
                .ToList();

            var users = _context.Users.ToList();

            ViewBag.Rooms = rooms;
            ViewBag.Users = users;
            ViewData["RoomTypes"] = roomTypes;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email or Password is incorrect");
            }

            return View(model);
        }
    }
}
