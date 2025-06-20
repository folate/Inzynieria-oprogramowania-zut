using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxiRideAPI.Models;
using TaxiRideAPI.Dtos;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkerController : ControllerBase
{
    private readonly TaxiRideDbContext _context;

    public WorkerController(TaxiRideDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] string login, [FromForm] string password, [FromForm] string name, [FromForm] string surname, [FromForm] string phoneNumber, [FromForm] string email, [FromForm] string department)
    {
        try
        {
            if (await _context.Workers.AnyAsync(w => w.login == login))
            {
                return BadRequest(new { Success = false, Message = "Login już istnieje" });
            }

            var worker = new Worker
            {
                login = login,
                password = password, // W rzeczywistej aplikacji powinno być zahashowane
                name = name,
                surname = surname,
                phoneNumber = phoneNumber,
                email = email,
                department = department
            };

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Rejestracja zakończona pomyślnie", WorkerId = worker.id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string login, [FromForm] string password)
    {
        try
        {
            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.login == login && w.password == password);
            
            if (worker == null)
            {
                return BadRequest(new { Success = false, Message = "Nieprawidłowy login lub hasło" });
            }

            if (!worker.isActive)
            {
                return BadRequest(new { Success = false, Message = "Konto jest nieaktywne" });
            }

            worker.lastActiveTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { 
                Success = true, 
                Message = "Logowanie zakończone pomyślnie", 
                WorkerId = worker.id,
                Worker = new {
                    id = worker.id,
                    login = worker.login,
                    name = worker.name,
                    surname = worker.surname,
                    email = worker.email,
                    department = worker.department
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(int id)
    {
        try
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null)
            {
                return NotFound(new { Success = false, Message = "Pracownik nie został znaleziony" });
            }

            return Ok(new { 
                Success = true, 
                Worker = new {
                    id = worker.id,
                    login = worker.login,
                    name = worker.name,
                    surname = worker.surname,
                    email = worker.email,
                    phoneNumber = worker.phoneNumber,
                    department = worker.department,
                    isActive = worker.isActive,
                    registrationDate = worker.registrationDate,
                    lastActiveTime = worker.lastActiveTime
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("tickets/assigned/{workerId}")]
    public async Task<IActionResult> GetAssignedTickets(int workerId)
    {
        try
        {
            var tickets = await _context.SupportTickets
                .Include(t => t.User)
                .Include(t => t.Ride)
                .Where(t => t.AssignedToWorkerId == workerId)
                .OrderByDescending(t => t.CreatedDate)
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

    [HttpGet("tickets/unassigned")]
    public async Task<IActionResult> GetUnassignedTickets()
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

    [HttpPost("ticket/{ticketId}/assign/{workerId}")]
    public async Task<IActionResult> AssignTicket(int ticketId, int workerId)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });
            }

            var worker = await _context.Workers.FindAsync(workerId);
            if (worker == null)
            {
                return NotFound(new { Success = false, Message = "Pracownik nie został znaleziony" });
            }

            ticket.AssignedToWorkerId = workerId;
            if (ticket.Status == "New")
            {
                ticket.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Zgłoszenie zostało przypisane do pracownika" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("ticket/{ticketId}/reply")]
    public async Task<IActionResult> ReplyToTicket(int ticketId, [FromBody] WorkerTicketReplyRequest request)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });
            }

            var message = new TicketMessage
            {
                TicketId = ticketId,
                SenderId = request.WorkerId,
                Message = request.Message,
                SentDate = DateTime.UtcNow,
                IsFromSupport = true
            };

            _context.TicketMessages.Add(message);

            // Update ticket status
            if (ticket.Status == "New")
            {
                ticket.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Odpowiedź została dodana" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("ticket/{ticketId}/status")]
    public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] WorkerTicketStatusRequest request)
    {
        try
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(new { Success = false, Message = "Zgłoszenie nie zostało znalezione" });
            }

            ticket.Status = request.Status;
            ticket.Resolution = request.Resolution;
            ticket.RefundAmount = request.RefundAmount;

            if (request.Status == "Resolved")
            {
                ticket.ResolvedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Status zgłoszenia został zaktualizowany" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetWorkerStatistics()
    {
        try
        {
            var totalTickets = await _context.SupportTickets.CountAsync();
            var openTickets = await _context.SupportTickets.CountAsync(t => t.Status == "New" || t.Status == "InProgress");
            var resolvedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved");
            var highPriorityTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "High" && (t.Status == "New" || t.Status == "InProgress"));

            return Ok(new { 
                Success = true, 
                Statistics = new {
                    totalTickets,
                    openTickets,
                    resolvedTickets,
                    highPriorityTickets
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}

public class WorkerTicketReplyRequest
{
    public int WorkerId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class WorkerTicketStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public double? RefundAmount { get; set; }
} 