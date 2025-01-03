using Microsoft.AspNetCore.Mvc;
using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Index()
        {
            var roomTypes = _context.Rooms
                .Where(r => r.Status == "Actif")  
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            var rooms = _context.Rooms
                .Where(r => r.Status == "Actif")
                .ToList();
            ViewBag.Rooms = rooms;

            var users = _context.Users.ToList();
            ViewBag.Users = users;

            ViewData["RoomTypes"] = roomTypes;


            return View();
        }
    }
}
