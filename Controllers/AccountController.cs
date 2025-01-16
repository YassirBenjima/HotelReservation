using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Login()
        {
            var roomTypes = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            var rooms = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .ToList();

            var users = _context.Users.ToList();

            ViewBag.Rooms = rooms;
            ViewBag.Users = users;
            ViewData["RoomTypes"] = roomTypes;

            return View();
        }

        public IActionResult Register()
        {
            var roomTypes = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            var rooms = _context.Rooms
                .Where(r => r.RoomStatus == "Actif")
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
                // Vérifier les informations de l'utilisateur dans la base de données
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == model.Email && c.Password == model.Password);

                if (client != null)
                {
                    // Créer le nom complet de l'utilisateur
                    var fullName = $"{client.FirstName} {client.LastName}";

                    // Créer une liste de claims, y compris l'ID de l'utilisateur
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fullName),               // Nom complet
                new Claim(ClaimTypes.Email, client.Email),          // Email
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()) // ID utilisateur
            };

                    // Créer l'identité avec les claims et spécifier le schéma d'authentification des cookies
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Créer un principal avec cette identité
                    var principal = new ClaimsPrincipal(identity);

                    // Se connecter en ajoutant le cookie d'authentification
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Rediriger vers la page d'accueil ou la page souhaitée
                    return RedirectToAction("Index", "Home");
                }

                // Si l'utilisateur n'est pas trouvé, ajouter une erreur
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
            }

            // Si le modèle n'est pas valide, retourner la vue avec les erreurs
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == model.Email);

                if (existingClient != null)
                {
                    ModelState.AddModelError("Email", "L'email est déjà utilisé.");
                    return View(model);
                }

                var client = new Client
                {
                    Email = model.Email,
                    Password = model.Password, 
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Address = model.Address
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                    new Claim(ClaimTypes.Email, client.Email)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
