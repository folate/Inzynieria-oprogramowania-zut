using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxiRideAPI.Dtos;
using TaxiRideAPI.Models;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SupportController : ControllerBase
{
    private readonly TaxiRideDbContext _context;

    public SupportController(TaxiRideDbContext context)
    {
        _context = context;
    }

    [HttpPost("ticket")]
    public async Task<IActionResult> CreateSupportTicket([FromBody] SupportTicketCreateDto request)
    {
        try
        {
            var ticketNumber = GenerateTicketNumber();

            var ticket = new SupportTicket
            {
                TicketNumber = ticketNumber,
                UserId = request.UserId,
                RideId = request.RideId,
                Subject = request.Subject,
                Description = request.Description,
                Priority = request.Priority,
                CreatedDate = DateTime.UtcNow,
                Status = "New"
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { 
                Success = true, 
                Message = "Zgłoszenie zostało utworzone", 
                TicketNumber = ticketNumber,
                TicketId = ticket.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("ticket/{ticketId}")]
    public async Task<IActionResult> GetTicket(int ticketId)
    {
        try
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Ride)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });

            var messages = await _context.TicketMessages
                .Include(m => m.Sender)
                .Where(m => m.TicketId == ticketId)
                .OrderBy(m => m.SentDate)
                .ToListAsync();

            return Ok(new { 
                Success = true, 
                Ticket = ticket,
                Messages = messages.Select(m => new {
                    m.Id,
                    m.Message,
                    m.SentDate,
                    m.IsFromSupport,
                    SenderName = m.IsFromSupport ? "Wsparcie techniczne" : $"{m.Sender?.firstName} {m.Sender?.lastName}"
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("tickets/{userId}")]
    public async Task<IActionResult> GetUserTickets(int userId)
    {
        try
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new {
                    t.Id,
                    t.TicketNumber,
                    t.Subject,
                    t.Priority,
                    t.Status,
                    t.CreatedDate,
                    t.ResolvedDate
                })
                .ToListAsync();

            return Ok(new { Success = true, Tickets = tickets });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("ticket/{ticketId}/message")]
    public async Task<IActionResult> AddMessageToTicket(int ticketId, [FromBody] TicketMessageDto request)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });

            var message = new TicketMessage
            {
                TicketId = ticketId,
                SenderId = request.SenderId,
                Message = request.Message,
                SentDate = DateTime.UtcNow,
                IsFromSupport = request.IsFromSupport
            };

            _context.TicketMessages.Add(message);

            // Aktualizuj status zgłoszenia
            if (request.IsFromSupport && ticket.Status == "New")
            {
                ticket.Status = "InProgress";
            }
            else if (!request.IsFromSupport && ticket.Status == "WaitingForUser")
            {
                ticket.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Wiadomość została dodana" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("ticket/message")]
    public async Task<IActionResult> AddMessage([FromBody] TicketMessageWithIdDto request)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId);
            if (ticket == null)
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });

            var message = new TicketMessage
            {
                TicketId = request.TicketId,
                SenderId = request.UserId,
                Message = request.Message,
                SentDate = DateTime.UtcNow,
                IsFromSupport = false // Domyślnie od użytkownika
            };

            _context.TicketMessages.Add(message);

            // Aktualizuj status zgłoszenia jeśli potrzeba
            if (ticket.Status == "WaitingForUser")
            {
                ticket.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Wiadomość została dodana" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("ticket/{ticketId}/status")]
    public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] SupportTicketUpdateDto request)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });

            ticket.Status = request.Status;
            ticket.Resolution = request.Resolution;
            ticket.RefundAmount = request.RefundAmount;
            ticket.AssignedToWorkerId = request.AssignedToWorkerId;

            if (request.Status == "Resolved")
            {
                ticket.ResolvedDate = DateTime.UtcNow;
                
                // Jeśli przyznano zwrot, zaktualizuj przejazd
                if (request.RefundAmount.HasValue && ticket.RideId.HasValue)
                {
                    await ProcessRefund(ticket.RideId.Value, request.RefundAmount.Value);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Status zgłoszenia został zaktualizowany" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("tickets/pending")]
    public async Task<IActionResult> GetPendingTickets()
    {
        try
        {
            var tickets = await _context.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Ride)
                .Where(t => t.AssignedToWorkerId == null && (t.Status == "New" || t.Status == "InProgress"))
                .OrderBy(t => t.Priority == "High" ? 0 : t.Priority == "Medium" ? 1 : 2)
                .ThenBy(t => t.CreatedDate)
                .Select(t => new {
                    t.Id,
                    t.TicketNumber,
                    t.Subject,
                    t.Priority,
                    t.Status,
                    t.CreatedDate,
                    t.ResolvedDate,
                    UserName = $"{t.User.firstName} {t.User.lastName}",
                    UserEmail = t.User.email,
                    RideId = t.RideId
                })
                .ToListAsync();

            return Ok(new { Success = true, Tickets = tickets });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private string GenerateTicketNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"TICKET-{date}-{random}";
    }

    private async Task ProcessRefund(int rideId, double refundAmount)
    {
        var ride = await _context.RideHistory.FindAsync(rideId);
        if (ride != null)
        {
            // Logika zwrotu - w rzeczywistej aplikacji byłaby to integracja z systemem płatności
            ride.Price -= refundAmount;
            if (ride.Price < 0) ride.Price = 0;
            
            // Dodaj komentarz o zwrocie
            ride.Comment = (ride.Comment ?? "") + $" [ZWROT: {refundAmount:C}]";
        }
    }
}
