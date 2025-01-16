using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class RoomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Details(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return RedirectToAction("Index", "Home");
        }
        var roomTypes = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();
        ViewData["RoomTypes"] = roomTypes;

        var rooms = _context.Rooms.Where(r => r.RoomType == type).ToList();

        if (rooms.Count == 0)
        {
            ViewBag.Message = "Aucune chambre disponible pour ce type.";
        }

        return View(rooms);
    }

    public IActionResult Book(int id)
    {
        var room = _context.Rooms.FirstOrDefault(r => r.RoomNumber == id);

        var roomTypes = _context.Rooms
              .Where(r => r.RoomStatus == "Actif")
              .Select(r => r.RoomType)
              .Distinct()
              .ToList();
        ViewData["RoomTypes"] = roomTypes;

        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

}
