using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace HotelReservation.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Book(int roomId, string roomType, DateTime checkIn, DateTime checkOut)
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var room = _context.Rooms.FirstOrDefault(r => r.RoomNumber == roomId);
            if (room == null)
            {
                return NotFound("Chambre introuvable.");
            }

            var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(clientIdClaim, out int clientId))
            {
                return BadRequest("L'identifiant utilisateur est invalide.");
            }

            var reservation = new Reservation
            {
                ReservationRoomNumber = room.RoomNumber,
                ReservationClientId = clientId,
                ReservationRoomType = roomType,
                ReservationIn = DateOnly.FromDateTime(checkIn),
                ReservationOut = DateOnly.FromDateTime(checkOut),
                ReservationClient = _context.Clients.FirstOrDefault(c => c.Id == clientId),
                ReservationRoomNumberNavigation = room
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("Confirmation", new { reservationId = reservation.ReservationId });
        }

        public IActionResult Confirmation(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.ReservationClient)
                .Include(r => r.ReservationRoomNumberNavigation)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                return NotFound("Réservation introuvable.");
            }

            var model = new ConfirmationViewModel
            {
                ReservationId = reservation.ReservationId,
                RoomType = reservation.ReservationRoomType,
                CheckIn = reservation.ReservationIn.ToDateTime(new TimeOnly(0, 0)),
                CheckOut = reservation.ReservationOut.ToDateTime(new TimeOnly(0, 0)),
                ClientName = $"{reservation.ReservationClient.FirstName} {reservation.ReservationClient.LastName}",
                TotalAmount = CalculateTotalAmount(reservation),
                Room_Image = reservation.ReservationRoomNumberNavigation.Room_Image
            };

            return View(model);
        }

        private decimal CalculateTotalAmount(Reservation reservation)
        {
            var roomRate = reservation.ReservationRoomNumberNavigation.RoomPrice;
            var numberOfNights = (reservation.ReservationOut.ToDateTime(new TimeOnly(0, 0)) - reservation.ReservationIn.ToDateTime(new TimeOnly(0, 0))).Days;
            return roomRate * numberOfNights;
        }

        [HttpGet]
        public IActionResult PaymentSuccess(int reservationId)
        {
            Console.WriteLine($"Searching for Reservation with ID: {reservationId}");

            var reservation = _context.Reservations
                .Include(r => r.ReservationClient)
                .Include(r => r.ReservationRoomNumberNavigation)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                Console.WriteLine($"Reservation with ID {reservationId} not found.");
                return NotFound("Réservation introuvable.");
            }

            var room = reservation.ReservationRoomNumberNavigation;

            if (room != null)
            {
                room.RoomStatus = "Non";
                _context.SaveChanges();
                Console.WriteLine($"Room {room.RoomNumber} status updated to {room.RoomStatus}");
            }

            var voucherContent = GenerateReservationVoucher(reservation);

            // Send Confirmation Email
            SendConfirmationEmail(reservation.ReservationClient.Email, voucherContent);

            // Generate and download the PDF reservation voucher
            var pdfStream = GenerateReservationVoucherPdf(reservation);

            // Return the PDF as a file for download
            return File(pdfStream, "application/pdf", "ReservationVoucher.pdf");
        }

        private string GenerateReservationVoucher(Reservation reservation)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Reservation Voucher");
            sb.AppendLine("-------------------");
            sb.AppendLine($"Reservation ID: {reservation.ReservationId}");
            sb.AppendLine($"Room Type: {reservation.ReservationRoomType}");
            sb.AppendLine($"Check-In Date: {reservation.ReservationIn.ToDateTime(new TimeOnly(0, 0)):yyyy-MM-dd}");
            sb.AppendLine($"Check-Out Date: {reservation.ReservationOut.ToDateTime(new TimeOnly(0, 0)):yyyy-MM-dd}");
            sb.AppendLine($"Client: {reservation.ReservationClient.FirstName} {reservation.ReservationClient.LastName}");
            sb.AppendLine($"Total Amount: {CalculateTotalAmount(reservation)} USD");
            sb.AppendLine("-------------------");
            sb.AppendLine("Thank you for choosing us!");

            return sb.ToString();
        }

        private MemoryStream GenerateReservationVoucherPdf(Reservation reservation)
        {
            var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Charger la police par défaut en gras
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Ajouter le contenu au document
            document.Add(new Paragraph("Reservation Voucher")
                .SetFont(font)
                .SetFontSize(18));
            document.Add(new Paragraph("-------------------"));
            document.Add(new Paragraph($"Reservation ID: {reservation.ReservationId}"));
            document.Add(new Paragraph($"Room Type: {reservation.ReservationRoomType}"));
            document.Add(new Paragraph($"Check-In Date: {reservation.ReservationIn.ToDateTime(new TimeOnly(0, 0)):yyyy-MM-dd}"));
            document.Add(new Paragraph($"Check-Out Date: {reservation.ReservationOut.ToDateTime(new TimeOnly(0, 0)):yyyy-MM-dd}"));
            document.Add(new Paragraph($"Client: {reservation.ReservationClient.FirstName} {reservation.ReservationClient.LastName}"));
            document.Add(new Paragraph($"Total Amount: {CalculateTotalAmount(reservation)} USD"));
            document.Add(new Paragraph("-------------------"));
            document.Add(new Paragraph("Thank you for choosing us!"));

            document.Close();
            memoryStream.Position = 0;

            return memoryStream;
        }

        private void SendConfirmationEmail(string recipientEmail, string voucherContent)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hotel Reservation", "your-email@domain.com"));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Reservation Confirmation";

            var body = new TextPart("plain")
            {
                Text = voucherContent
            };

            message.Body = body;

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect("sandbox.smtp.mailtrap.io", 587, SecureSocketOptions.StartTls);
                    client.Authenticate("6c8352358343fc", "8bd6571eb4cd04");
                    client.Send(message);
                    Console.WriteLine("Confirmation email sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }

    }
}
