using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;

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

            if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out int clientId))
            {
                return RedirectToAction("Login", "Account");
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

            try
            {
                _context.Reservations.Add(reservation);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving reservation: {ex.Message}");
                return StatusCode(500, "Erreur lors de la création de la réservation.");
            }

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

        [HttpGet]
        public IActionResult PaymentSuccess(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.ReservationClient)
                .Include(r => r.ReservationRoomNumberNavigation)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                return NotFound("Réservation introuvable.");
            }

            var room = reservation.ReservationRoomNumberNavigation;

            if (room != null)
            {
                room.RoomStatus = "Non";
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating room status: {ex.Message}");
                }
            }

            // Génération du PDF
            var pdfStream = GenerateReservationVoucherPdf(reservation);

            if (pdfStream == null || pdfStream.Length == 0)
            {
                Console.WriteLine("Error: PDF not generated or empty.");
                return StatusCode(500, "Erreur lors de la génération du PDF.");
            }

            // Ensure that the stream is at the beginning before returning the file
            pdfStream.Position = 0;

            try
            {
                SendConfirmationEmailWithAttachment(reservation.ReservationClient.Email, pdfStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }

            // Return PDF file for download
            return File(pdfStream, "application/pdf", "ReservationVoucher.pdf");
        }


        private decimal CalculateTotalAmount(Reservation reservation)
        {
            var roomRate = reservation.ReservationRoomNumberNavigation.RoomPrice;
            var numberOfNights = (reservation.ReservationOut.ToDateTime(new TimeOnly(0, 0)) - reservation.ReservationIn.ToDateTime(new TimeOnly(0, 0))).Days;
            return roomRate * numberOfNights;
        }

        private MemoryStream GenerateReservationVoucherPdf(Reservation reservation)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                document.Add(new Paragraph("Reservation Voucher").SetFont(font).SetFontSize(18));
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

                // Save the PDF to disk for debugging
                using (var fileStream = new FileStream("ReservationVoucherTest.pdf", FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }

                Console.WriteLine("PDF generated and saved successfully.");
                return memoryStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                return null;
            }
        }

        public void SendConfirmationEmailWithAttachment(string recipientEmail, MemoryStream pdfStream)
        {
            try
            {
                Console.WriteLine("Préparation de l'email...");
                // Créez le message de l'email avec MimeKit
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("From Name", "from@example.com"));
                emailMessage.To.Add(new MailboxAddress("To Name", recipientEmail));
                emailMessage.Subject = "Reservation Confirmation";

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = "Dear Customer,\n\nYour reservation has been confirmed. Please find the attached voucher."
                };

                // Attacher le PDF
                pdfStream.Position = 0; // Réinitialiser la position du flux avant de l'attacher
                bodyBuilder.Attachments.Add("ReservationVoucher.pdf", pdfStream.ToArray(), new ContentType("application", "pdf"));

                emailMessage.Body = bodyBuilder.ToMessageBody();

                // Envoi de l'email avec MailKit
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("sandbox.smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);
                    smtpClient.Authenticate("6c8352358343fc", "8bd6571eb4cd04");
                    smtpClient.Send(emailMessage);
                    smtpClient.Disconnect(true);
                }

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
