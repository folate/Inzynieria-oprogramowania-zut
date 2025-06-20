using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TaxiRideAPI.Dtos;
using TaxiRideAPI.Models;
using System.IO;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly TaxiRideDbContext _context;

    public ReportsController(TaxiRideDbContext context)
    {
        _context = context;
    }

    [HttpPost("financial")]
    public async Task<IActionResult> GenerateFinancialReport([FromBody] FinancialReportRequestDto request)
    {
        try
        {
            // Konwertuj daty na UTC jeśli nie mają Kind
            var startDateUtc = request.StartDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc) 
                : request.StartDate.ToUniversalTime();
            var endDateUtc = request.EndDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc) 
                : request.EndDate.ToUniversalTime();
            
            var rides = await _context.RideHistory
                .Include(r => r.User)
                .Include(r => r.Rider)
                .Where(r => r.OrderTime >= startDateUtc && 
                           r.OrderTime <= endDateUtc && 
                           r.Status == "Completed" && 
                           r.IsPaid)
                .ToListAsync();

            var totalRevenue = rides.Sum(r => r.Price);
            var totalCommission = totalRevenue * 0.15; // 15% prowizji
            var totalVat = request.IncludeVat ? totalRevenue * 0.23 : 0; // 23% VAT

            // Utwórz raport
            var report = new FinancialReport
            {
                StartDate = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Utc),
                TotalRevenue = totalRevenue,
                TotalCommission = totalCommission,
                TotalVat = totalVat,
                TotalRides = rides.Count,
                ReportType = request.ReportType,
                GeneratedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                GeneratedBy = 1, // TODO: Pobrać z aktualnie zalogowanego użytkownika
                FilePath = ""
            };

            _context.FinancialReports.Add(report);
            await _context.SaveChangesAsync();

            // Generuj plik raportu
            var fileName = $"financial_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.ExportFormat?.ToLower() ?? "pdf"}";
            var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            var filePath = Path.Combine(reportsDirectory, fileName);
            
            // Upewnij się, że katalog istnieje
            Directory.CreateDirectory(reportsDirectory);

            try
            {
                if (request.ExportFormat?.ToUpper() == "CSV")
                {
                    await GenerateCsvReport(rides, filePath, report);
                }
                else
                {
                    await GeneratePdfReport(rides, filePath, report);
                }

                report.FilePath = filePath;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // If file generation fails, remove the report from database
                _context.FinancialReports.Remove(report);
                await _context.SaveChangesAsync();
                throw new Exception($"Błąd podczas generowania pliku raportu: {ex.Message}");
            }

            return Ok(new { 
                Success = true, 
                Message = "Raport został wygenerowany", 
                ReportId = report.Id,
                FilePath = filePath,
                Summary = new {
                    TotalRides = rides.Count,
                    TotalRevenue = totalRevenue,
                    TotalCommission = totalCommission,
                    TotalVat = totalVat,
                    NetRevenue = totalRevenue - totalCommission - totalVat
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("financial/{reportId}")]
    public async Task<IActionResult> GetFinancialReport(int reportId)
    {
        try
        {
            var report = await _context.FinancialReports.FindAsync(reportId);
            if (report == null)
                return NotFound(new { Success = false, Message = "Raport nie został znaleziony" });

            return Ok(new { Success = true, Report = report });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("download/{reportId}")]
    public async Task<IActionResult> DownloadReport(int reportId)
    {
        try
        {
            var report = await _context.FinancialReports.FindAsync(reportId);
            if (report == null)
            {
                return NotFound(new { Success = false, Message = $"Raport o ID {reportId} nie został znaleziony" });
            }

            if (string.IsNullOrEmpty(report.FilePath))
            {
                return NotFound(new { Success = false, Message = "Ścieżka do pliku raportu jest pusta" });
            }

            // Use absolute path
            var absolutePath = Path.IsPathRooted(report.FilePath) 
                ? report.FilePath 
                : Path.Combine(Directory.GetCurrentDirectory(), report.FilePath);

            if (!System.IO.File.Exists(absolutePath))
            {
                return NotFound(new { 
                    Success = false, 
                    Message = $"Plik raportu nie został znaleziony: {absolutePath}" 
                });
            }

            var fileName = Path.GetFileName(report.FilePath);
            var contentType = fileName.EndsWith(".pdf") ? "application/pdf" : "text/csv";
            
            var fileBytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = $"Błąd podczas pobierania raportu: {ex.Message}" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserReports(int userId)
    {
        try
        {
            // Pobierz raporty użytkownika (w tym przypadku po prostu wszystkie raporty)
            var reports = await _context.FinancialReports
                .Where(r => r.GeneratedBy == userId || userId == 1) // Admin widzi wszystkie
                .OrderByDescending(r => r.GeneratedDate)
                .Select(r => new {
                    r.Id,
                    r.ReportType,
                    r.GeneratedDate,
                    r.StartDate,
                    r.EndDate,
                    r.TotalRevenue,
                    r.TotalRides
                })
                .ToListAsync();

            return Ok(new { Success = true, Reports = reports });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var reports = await _context.FinancialReports
                .OrderByDescending(r => r.GeneratedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalReports = await _context.FinancialReports.CountAsync();

            return Ok(new { 
                Success = true, 
                Reports = reports,
                TotalReports = totalReports,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalReports / pageSize)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        try
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            // Dane za dzisiaj
            var todayRides = await _context.RideHistory
                .Where(r => r.OrderTime.Date == today && r.Status == "Completed")
                .ToListAsync();

            // Dane za ten miesiąc
            var thisMonthRides = await _context.RideHistory
                .Where(r => r.OrderTime >= thisMonth && r.Status == "Completed")
                .ToListAsync();

            // Dane za ostatni miesiąc
            var lastMonthRides = await _context.RideHistory
                .Where(r => r.OrderTime >= lastMonth && r.OrderTime < thisMonth && r.Status == "Completed")
                .ToListAsync();

            // Aktywni kierowcy
            var activeDrivers = await _context.Riders
                .Where(r => r.isAvailable && r.lastActiveTime > DateTime.UtcNow.AddHours(-2))
                .CountAsync();

            // Najlepsi kierowcy
            var topDrivers = await _context.Riders
                .Where(r => r.totalRides > 0)
                .OrderByDescending(r => r.starRating)
                .ThenByDescending(r => r.totalRides)
                .Take(5)
                .ToListAsync();

            return Ok(new { 
                Success = true, 
                Dashboard = new {
                    Today = new {
                        TotalRides = todayRides.Count,
                        TotalRevenue = todayRides.Sum(r => r.Price),
                        AverageRideValue = todayRides.Any() ? todayRides.Average(r => r.Price) : 0
                    },
                    ThisMonth = new {
                        TotalRides = thisMonthRides.Count,
                        TotalRevenue = thisMonthRides.Sum(r => r.Price),
                        AverageRideValue = thisMonthRides.Any() ? thisMonthRides.Average(r => r.Price) : 0
                    },
                    LastMonth = new {
                        TotalRides = lastMonthRides.Count,
                        TotalRevenue = lastMonthRides.Sum(r => r.Price),
                        AverageRideValue = lastMonthRides.Any() ? lastMonthRides.Average(r => r.Price) : 0
                    },
                    ActiveDrivers = activeDrivers,
                    TopDrivers = topDrivers.Select(d => new {
                        d.id,
                        Name = $"{d.name} {d.surname}",
                        d.starRating,
                        d.totalRides,
                        d.totalEarnings
                    })
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private async Task GenerateCsvReport(List<Ride> rides, string filePath, FinancialReport report)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Data,Klient,Kierowca,Początek,Koniec,Cena,Prowizja,Status");

        foreach (var ride in rides)
        {
            var commission = ride.Price * 0.15;
            csv.AppendLine($"{ride.OrderTime:yyyy-MM-dd},{ride.User?.firstName} {ride.User?.lastName},{ride.Rider?.name} {ride.Rider?.surname},{ride.PickupLocation},{ride.DropoffLocation},{ride.Price:C},{commission:C},{ride.Status}");
        }

        csv.AppendLine($"PODSUMOWANIE,,,,,,");
        csv.AppendLine($"Łączna liczba przejazdów,{report.TotalRides},,,,");
        csv.AppendLine($"Łączny przychód,{report.TotalRevenue:C},,,,");
        csv.AppendLine($"Łączna prowizja,{report.TotalCommission:C},,,,");
        csv.AppendLine($"VAT,{report.TotalVat:C},,,,");

        await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
    }

    private async Task GeneratePdfReport(List<Ride> rides, string filePath, FinancialReport report)
    {
        // Simplified PDF generation - w rzeczywistej aplikacji użyłbyś biblioteki jak iTextSharp
        var content = new StringBuilder();
        content.AppendLine("RAPORT FINANSOWY");
        content.AppendLine($"Okres: {report.StartDate:yyyy-MM-dd} - {report.EndDate:yyyy-MM-dd}");
        content.AppendLine($"Wygenerowano: {report.GeneratedDate:yyyy-MM-dd HH:mm}");
        content.AppendLine("");
        content.AppendLine("PODSUMOWANIE:");
        content.AppendLine($"Łączna liczba przejazdów: {report.TotalRides}");
        content.AppendLine($"Łączny przychód: {report.TotalRevenue:C}");
        content.AppendLine($"Łączna prowizja: {report.TotalCommission:C}");
        content.AppendLine($"VAT: {report.TotalVat:C}");
        content.AppendLine($"Przychód netto: {(report.TotalRevenue - report.TotalCommission - report.TotalVat):C}");
        content.AppendLine("");
        content.AppendLine("SZCZEGÓŁY PRZEJAZDÓW:");
        
        foreach (var ride in rides.Take(100)) // Ograniczenie do 100 przejazdów
        {
            content.AppendLine($"{ride.OrderTime:yyyy-MM-dd HH:mm} | {ride.User?.firstName} {ride.User?.lastName} | {ride.PickupLocation} -> {ride.DropoffLocation} | {ride.Price:C}");
        }

        await System.IO.File.WriteAllTextAsync(filePath, content.ToString(), Encoding.UTF8);
    }
}
