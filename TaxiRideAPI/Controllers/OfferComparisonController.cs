using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxiRideAPI.Dtos;
using TaxiRideAPI.Models;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OfferComparisonController : ControllerBase
{
    private readonly TaxiRideDbContext _context;

    public OfferComparisonController(TaxiRideDbContext context)
    {
        _context = context;
    }

    [HttpPost("compare")]
    public async Task<IActionResult> CompareOffers([FromBody] OfferComparisonDto request)
    {
        try
        {
            // Zapisz żądanie porównania
            var comparisonRequest = new ComparisonRequest
            {
                UserId = request.UserId,
                PickupLocation = request.PickupLocation,
                DropoffLocation = request.DropoffLocation,
                RequestTime = DateTime.UtcNow,
                ScheduledTime = request.ScheduledTime?.ToUniversalTime(),
                VehicleTypeFilter = request.VehicleTypeFilter,
                SortBy = request.SortBy
            };

            _context.ComparisonRequests.Add(comparisonRequest);

            // Pobierz dostępnych kierowców
            var availableRiders = await _context.Riders
                .Where(r => r.isAvailable && r.isVerified)
                .ToListAsync();

            // Pobierz firmy taksówkarskie
            var companies = await _context.TaxiCompanies
                .Where(c => c.IsActive)
                .ToListAsync();

            var offers = new List<OfferResponseDto>();

            // Generuj oferty dla każdego dostępnego kierowcy
            foreach (var rider in availableRiders)
            {
                var company = companies.FirstOrDefault(c => c.Id == rider.companyId);
                
                // Oblicz cenę bazową (przykładowa logika)
                var basePrice = CalculateBasePrice(request.PickupLocation, request.DropoffLocation);
                var estimatedTime = CalculateEstimatedTime(request.PickupLocation, request.DropoffLocation);

                // Dodaj modyfikatory ceny dla różnych typów pojazdów
                var vehicleType = GetVehicleType(rider.id);
                var priceMultiplier = vehicleType switch
                {
                    "Comfort" => 1.2,
                    "Premium" => 1.5,
                    "Van" => 1.3,
                    _ => 1.0
                };

                if (request.ComfortOnly && vehicleType == "Standard")
                    continue;

                var finalPrice = basePrice * priceMultiplier;

                // Utwórz ofertę
                var rideOffer = new RideOffer
                {
                    RideId = 0, // Będzie ustawione po utworzeniu przejazdu
                    RiderId = rider.id,
                    Price = finalPrice,
                    EstimatedTime = estimatedTime,
                    IsAccepted = false,
                    VehicleType = vehicleType,
                    DriverRating = rider.starRating,
                    CompanyId = rider.companyId
                };

                _context.RideOffers.Add(rideOffer);

                offers.Add(new OfferResponseDto
                {
                    Id = rideOffer.Id,
                    RiderId = rider.id,
                    RiderName = $"{rider.name} {rider.surname}",
                    Price = finalPrice,
                    EstimatedTime = estimatedTime,
                    VehicleType = vehicleType,
                    DriverRating = rider.starRating,
                    CompanyName = company?.Name ?? "Niezależny",
                    VehicleModel = GetVehicleModel(rider.id),
                    VehicleColor = GetVehicleColor(rider.id),
                    IsAvailable = rider.isAvailable
                });
            }

            // Sortuj oferty według wybranego kryterium
            offers = request.SortBy switch
            {
                "Price" => offers.OrderBy(o => o.Price).ToList(),
                "Time" => offers.OrderBy(o => o.EstimatedTime).ToList(),
                "Rating" => offers.OrderByDescending(o => o.DriverRating).ToList(),
                _ => offers.OrderBy(o => o.Price).ToList()
            };

            await _context.SaveChangesAsync();

            return Ok(new { 
                Success = true, 
                Message = "Oferty zostały wygenerowane", 
                Offers = offers,
                TotalOffers = offers.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetComparisonHistory(int userId)
    {
        try
        {
            var history = await _context.ComparisonRequests
                .Where(cr => cr.UserId == userId)
                .OrderByDescending(cr => cr.RequestTime)
                .Take(10)
                .ToListAsync();

            return Ok(new { Success = true, History = history });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private double CalculateBasePrice(string pickup, string dropoff)
    {
        // Przykładowa logika obliczania ceny bazowej
        var baseRate = 3.50; // Stawka bazowa
        var perKmRate = 2.20; // Stawka za km
        var estimatedDistance = 5.0; // Przykładowa odległość w km
        
        return baseRate + (estimatedDistance * perKmRate);
    }

    private int CalculateEstimatedTime(string pickup, string dropoff)
    {
        // Przykładowa logika obliczania czasu
        return new Random().Next(10, 45); // 10-45 minut
    }

    private string GetVehicleType(int riderId)
    {
        var vehicle = _context.Vehicles.FirstOrDefault(v => v.RiderId == riderId && v.IsActive);
        return vehicle?.VehicleType ?? "Standard";
    }

    private string GetVehicleModel(int riderId)
    {
        var vehicle = _context.Vehicles.FirstOrDefault(v => v.RiderId == riderId && v.IsActive);
        return vehicle != null ? $"{vehicle.Make} {vehicle.Model}" : "Nieznany";
    }

    private string GetVehicleColor(int riderId)
    {
        var vehicle = _context.Vehicles.FirstOrDefault(v => v.RiderId == riderId && v.IsActive);
        return vehicle?.Color ?? "Nieznany";
    }
}
