using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxiRideAPI.Models;
using TaxiRideAPI.Dtos;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OwnerController : ControllerBase
{
    private readonly TaxiRideDbContext _context;

    public OwnerController(TaxiRideDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] string login, [FromForm] string password, [FromForm] string name, [FromForm] string surname, [FromForm] string phoneNumber, [FromForm] string email)
    {
        try
        {
            if (await _context.Owners.AnyAsync(o => o.login == login))
            {
                return BadRequest(new { Success = false, Message = "Login już istnieje" });
            }

            var owner = new Owner
            {
                login = login,
                password = password, // W rzeczywistej aplikacji powinno być zahashowane
                name = name,
                surname = surname,
                phoneNumber = phoneNumber,
                email = email
            };

            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Rejestracja zakończona pomyślnie", OwnerId = owner.id });
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
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.login == login && o.password == password);
            
            if (owner == null)
            {
                return BadRequest(new { Success = false, Message = "Nieprawidłowy login lub hasło" });
            }

            if (!owner.isActive)
            {
                return BadRequest(new { Success = false, Message = "Konto jest nieaktywne" });
            }

            owner.lastActiveTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { 
                Success = true, 
                Message = "Logowanie zakończone pomyślnie", 
                OwnerId = owner.id,
                Owner = new {
                    id = owner.id,
                    login = owner.login,
                    name = owner.name,
                    surname = owner.surname,
                    email = owner.email,
                    companyId = owner.companyId
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
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
            {
                return NotFound(new { Success = false, Message = "Właściciel nie został znaleziony" });
            }

            return Ok(new { 
                Success = true, 
                Owner = new {
                    id = owner.id,
                    login = owner.login,
                    name = owner.name,
                    surname = owner.surname,
                    email = owner.email,
                    phoneNumber = owner.phoneNumber,
                    companyId = owner.companyId,
                    isActive = owner.isActive,
                    registrationDate = owner.registrationDate,
                    lastActiveTime = owner.lastActiveTime
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("drivers/{companyId}")]
    public async Task<IActionResult> GetDrivers(int companyId)
    {
        try
        {
            var drivers = await _context.Riders
                .Where(r => r.companyId == companyId)
                .Select(r => new {
                    r.id,
                    r.name,
                    r.surname,
                    r.email,
                    r.phoneNumber,
                    r.starRating,
                    r.totalRides,
                    r.totalEarnings,
                    r.isAvailable,
                    r.isVerified,
                    r.lastActiveTime
                })
                .ToListAsync();

            return Ok(new { Success = true, Drivers = drivers });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("financial-report")]
    public async Task<IActionResult> GenerateFinancialReport([FromBody] FinancialReportRequest request)
    {
        try
        {
            var startDate = DateTime.Parse(request.StartDate);
            var endDate = DateTime.Parse(request.EndDate);

            // Get rides for the company's drivers in the date range
            var rides = await _context.RideHistory
                .Include(r => r.Rider)
                .Where(r => r.Rider != null && r.Rider.companyId == request.CompanyId)
                .Where(r => r.OrderTime >= startDate && r.OrderTime <= endDate)
                .ToListAsync();

            var totalRevenue = rides.Sum(r => r.Price);
            var totalRides = rides.Count;
            var totalVat = request.IncludeVat ? totalRevenue * 0.23 : 0; // 23% VAT
            var totalCommission = totalRevenue * 0.15; // 15% commission

            // Generate report file
            var reportId = Guid.NewGuid().ToString();
            var fileName = $"financial_report_{DateTime.Now:yyyyMMdd_HHmmss}.{request.ExportFormat.ToLower()}";
            var filePath = Path.Combine("Reports", fileName);

            // Ensure Reports directory exists
            Directory.CreateDirectory("Reports");

            // Create report content
            var reportContent = GenerateReportContent(rides, totalRevenue, totalCommission, totalVat, totalRides, request);

            // Write file
            await System.IO.File.WriteAllTextAsync(filePath, reportContent);

            // Save report record
            var report = new FinancialReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                TotalCommission = totalCommission,
                TotalVat = totalVat,
                TotalRides = totalRides,
                ReportType = request.ReportType,
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = request.OwnerId,
                FilePath = filePath
            };

            _context.FinancialReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { 
                Success = true, 
                Message = "Raport został wygenerowany",
                ReportId = report.Id,
                FileName = fileName,
                Report = new {
                    id = report.Id,
                    startDate = report.StartDate,
                    endDate = report.EndDate,
                    totalRevenue = report.TotalRevenue,
                    totalCommission = report.TotalCommission,
                    totalVat = report.TotalVat,
                    totalRides = report.TotalRides,
                    reportType = report.ReportType,
                    generatedDate = report.GeneratedDate,
                    fileName = fileName
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("download-report/{reportId}")]
    public async Task<IActionResult> DownloadReport(int reportId)
    {
        try
        {
            var report = await _context.FinancialReports.FindAsync(reportId);
            if (report == null)
            {
                return NotFound(new { Success = false, Message = "Raport nie został znaleziony" });
            }

            if (!System.IO.File.Exists(report.FilePath))
            {
                return NotFound(new { Success = false, Message = "Plik raportu nie został znaleziony" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(report.FilePath);
            var fileName = Path.GetFileName(report.FilePath);

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private string GenerateReportContent(List<Ride> rides, double totalRevenue, double totalCommission, double totalVat, int totalRides, FinancialReportRequest request)
    {
        var content = $"RAPORT FINANSOWY\n";
        content += $"Typ raportu: {request.ReportType}\n";
        content += $"Okres: {request.StartDate} - {request.EndDate}\n";
        content += $"Data wygenerowania: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";
        
        content += $"PODSUMOWANIE:\n";
        content += $"Łączna liczba przejazdów: {totalRides}\n";
        content += $"Łączny przychód: {totalRevenue:C}\n";
        content += $"Łączna prowizja: {totalCommission:C}\n";
        if (request.IncludeVat)
        {
            content += $"Łączny VAT: {totalVat:C}\n";
        }
        content += $"Przychód netto: {(totalRevenue - totalCommission):C}\n\n";
        
        content += $"SZCZEGÓŁY PRZEJAZDÓW:\n";
        foreach (var ride in rides)
        {
            content += $"ID: {ride.Id}, Data: {ride.OrderTime:yyyy-MM-dd HH:mm}, Cena: {ride.Price:C}, Status: {ride.Status}\n";
        }
        
        return content;
    }
}

public class FinancialReportRequest
{
    public int OwnerId { get; set; }
    public int CompanyId { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public bool IncludeVat { get; set; } = false;
    public string ExportFormat { get; set; } = "TXT";
} 