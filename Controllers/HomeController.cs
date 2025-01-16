using Microsoft.AspNetCore.Mvc;
using HotelReservation.Data;
using HotelReservation.Models;  // Assurez-vous d'inclure le modèle Client si nécessaire
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

        public IActionResult Gallery()
        {
            var roomTypes = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            var rooms = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .ToList();

            ViewBag.Rooms = rooms;
            ViewData["RoomTypes"] = roomTypes;

            return View();
        }

        public IActionResult Index()
        {

            var roomTypes = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            var rooms = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .ToList();

            foreach (var room in rooms)
            {
                if (room.Room_Image == null)
                {
                    room.Room_Image = new byte[0]; 
                }
            }
            var users = _context.Users.ToList();
            ViewBag.Users = users;
            ViewBag.TotalUsers = users.Count();

            var clients = _context.Clients.ToList(); 
            ViewBag.TotalClients = clients.Count();

            var reservations = _context.Reservations.ToList();
            ViewBag.TotalReservations = reservations.Count();

            ViewBag.TotalRooms = rooms.Count();
            ViewBag.Rooms = rooms;
            ViewData["RoomTypes"] = roomTypes;

            return View();
        }

        public IActionResult Hebergement()
        {
            var rooms = _context.Rooms.ToList();

            var roomTypes = _context.Rooms
               .Where(r => r.RoomStatus == "Actif")
               .Select(r => r.RoomType)
               .Distinct()
               .ToList();
            ViewData["RoomTypes"] = roomTypes;
            ViewBag.RoomTypes = roomTypes;

            return View(rooms);
        }
    }
}
