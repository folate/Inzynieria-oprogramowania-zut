using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxiRideAPI.Dtos;
using TaxiRideAPI.Models;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RiderController : ControllerBase
{
    private readonly TaxiRideDbContext _context;
    public RiderController(TaxiRideDbContext dbContext)
    {
        _context = dbContext;
    }
    
    [HttpGet("riders")]
    public IActionResult GetRiders()
    {
        var ridersDto = new List<RiderDto>();
        var riders = _context.Riders.ToList();
        foreach (var rider in riders)
        {
            ridersDto.Add(new RiderDto
            {
                Id = rider.id,
                Login = rider.login,
                Name = rider.name,
                Surname = rider.surname,
                PhoneNumber = rider.phoneNumber,
                Email = rider.email,
                StarRating = rider.starRating
            });
        }
        return Ok(ridersDto);
    }
    
    [HttpPost("login")]
    public IActionResult Login([FromForm] string login, [FromForm] string password)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.login == login);
        if (rider != null && rider.password == password)
        {
            return Ok(new { message = $"Hello {rider.name} {rider.surname}!", riderId = rider.id });
        }
        return Unauthorized("Unauthorized");
    }
    
    [HttpPost("register")]
    public IActionResult Register([FromForm] string login, [FromForm] string password, 
                                  [FromForm] string name, [FromForm] string surname, 
                                  [FromForm] string phoneNumber, [FromForm] string email,
                                  [FromForm] string licenseNumber, [FromForm] string vehicleInfo)
    {
        // Check if rider already exists
        if (_context.Riders.Any(r => r.login == login))
            return BadRequest("Username is taken");
        
        var newRider = new Rider
        {
            login = login,
            password = password,
            name = name,
            surname = surname,
            phoneNumber = phoneNumber,
            email = email,
            licenseNumber = licenseNumber,
            vehicleInfo = vehicleInfo,
            starRating = 5.0,
            isAvailable = false,
            licenseExpiryDate = DateTime.UtcNow.AddYears(5), // Default 5 years
            lastActiveTime = DateTime.UtcNow
        };
        
        _context.Riders.Add(newRider);
        if (_context.SaveChanges() > 0)
            return Ok(new { message = $"Rider {name} {surname} registered successfully!", riderId = newRider.id });
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpGet("profile/{id}")]
    public IActionResult GetProfile(int id)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == id);
        if (rider == null)
            return NotFound("Rider not found");
        
        return Ok(new RiderDto
        {
            Id = rider.id,
            Login = rider.login,
            Name = rider.name,
            Surname = rider.surname,
            PhoneNumber = rider.phoneNumber,
            Email = rider.email,
            StarRating = rider.starRating
        });
    }
    
    [HttpPost("modify")]
    public IActionResult ModifyRider([FromForm] int id, [FromForm] string name, [FromForm] string surname, 
                                     [FromForm] string phoneNumber, [FromForm] string email)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == id);
        if (rider == null)
            return NotFound("Rider not found");
        
        rider.name = name ?? rider.name;
        rider.surname = surname ?? rider.surname;
        rider.phoneNumber = phoneNumber ?? rider.phoneNumber;
        rider.email = email ?? rider.email;
        
        if (_context.SaveChanges() > 0)
            return Ok($"Rider {rider.name} {rider.surname} modified successfully!");
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpPost("availability")]
    public IActionResult UpdateAvailability([FromForm] DriverAvailabilityDto availabilityDto)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == availabilityDto.RiderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        rider.isAvailable = availabilityDto.IsAvailable;
        
        if (availabilityDto.IsAvailable)
        {
            rider.latitude = availabilityDto.Latitude;
            rider.longitude = availabilityDto.Longitude;
            rider.lastLocationUpdate = DateTime.UtcNow;
            
            // Also update the availability schedule
            var availability = new DriverAvailability
            {
                RiderId = rider.id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(8), // Default shift of 8 hours
                IsAvailable = true
            };
            
            _context.DriverAvailabilities.Add(availability);
        }
        
        if (_context.SaveChanges() > 0)
            return Ok($"Availability updated successfully. Status: {(availabilityDto.IsAvailable ? "Available" : "Unavailable")}");
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpGet("rides/{riderId}")]
    public IActionResult GetRideHistory(int riderId)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == riderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        var rides = _context.RideHistory
            .Include(r => r.User)
            .Where(r => r.RiderId == riderId)
            .OrderByDescending(r => r.OrderTime)
            .ToList();
        
        var rideHistoryDtos = rides.Select(r => new RideHistoryDto
        {
            Id = r.Id,
            PickupLocation = r.PickupLocation,
            DropoffLocation = r.DropoffLocation,
            OrderTime = r.OrderTime,
            PickupTime = r.PickupTime,
            DropoffTime = r.DropoffTime,
            Price = r.Price,
            Status = r.Status,
            Rating = r.Rating,
            RiderName = r.User?.login ?? "Unknown",
            IsPaid = r.IsPaid,
            PaymentMethod = r.PaymentMethod ?? "Unknown"
        }).ToList();
        
        return Ok(rideHistoryDtos);
    }
    
    [HttpGet("statistics/{riderId}")]
    public IActionResult GetStatistics(int riderId)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == riderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        var completedRides = _context.RideHistory.Count(r => r.RiderId == riderId && r.Status == "Completed");
        var cancelledRides = _context.RideHistory.Count(r => r.RiderId == riderId && r.Status == "Cancelled");
        var totalEarnings = _context.RideHistory
            .Where(r => r.RiderId == riderId && r.Status == "Completed" && r.IsPaid)
            .Sum(r => r.Price);
        
        var ratings = _context.RideHistory
            .Where(r => r.RiderId == riderId && r.Rating.HasValue)
            .Select(r => r.Rating!.Value)
            .ToList();
        
        var averageRating = ratings.Any() ? ratings.Average() : 0;
        
        return Ok(new
        {
            TotalRides = completedRides + cancelledRides,
            CompletedRides = completedRides,
            CancelledRides = cancelledRides,
            TotalEarnings = totalEarnings,
            AverageRating = averageRating,
            TotalRatings = ratings.Count
        });
    }
    
    [HttpGet("pending-rides/{riderId}")]
    public IActionResult GetPendingRides(int riderId)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == riderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        var pendingRides = _context.RideHistory
            .Include(r => r.User)
            .Where(r => r.RiderId == riderId && (r.Status == "Accepted" || r.Status == "InProgress"))
            .OrderBy(r => r.OrderTime)
            .ToList();
        
        var pendingRideDtos = pendingRides.Select(r => new RideHistoryDto
        {
            Id = r.Id,
            PickupLocation = r.PickupLocation,
            DropoffLocation = r.DropoffLocation,
            OrderTime = r.OrderTime,
            PickupTime = r.PickupTime,
            Status = r.Status,
            RiderName = r.User?.login ?? "Nieznany",
            PaymentMethod = r.PaymentMethod ?? "Brak",
            DropoffTime = r.DropoffTime,
            Price = r.Price,
            Rating = r.Rating,
            IsPaid = r.IsPaid
        }).ToList();
        
        return Ok(pendingRideDtos);
    }
    
    [HttpGet("available-rides/{riderId}")]
    public IActionResult GetAvailableRides(int riderId)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == riderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        // Get rides that are pending and have offers for this rider
        var availableRides = _context.RideHistory
            .Include(r => r.User)
            .Where(r => r.Status == "Pending" && r.RiderId == null)
            .OrderBy(r => r.OrderTime)
            .ToList();
        
        var availableRideDtos = availableRides.Select(r => new RideHistoryDto
        {
            Id = r.Id,
            PickupLocation = r.PickupLocation,
            DropoffLocation = r.DropoffLocation,
            OrderTime = r.OrderTime,
            PickupTime = r.PickupTime,
            Status = r.Status,
            RiderName = r.User?.login ?? "Nieznany",
            PaymentMethod = r.PaymentMethod ?? "Brak",
            DropoffTime = r.DropoffTime,
            Price = r.Price,
            Rating = r.Rating,
            IsPaid = r.IsPaid
        }).ToList();
        
        return Ok(availableRideDtos);
    }
    
    [HttpPost("ride/status")]
    public IActionResult UpdateRideStatus([FromForm] RideStatusUpdateDto statusDto)
    {
        try
        {
            if (statusDto == null)
            {
                return BadRequest("Status data is null");
            }

            var ride = _context.RideHistory.FirstOrDefault(r => r.Id == statusDto.RideId);
            if (ride == null)
            {
                return NotFound($"Ride with ID {statusDto.RideId} not found");
            }

            var oldStatus = ride.Status;
            ride.Status = statusDto.Status;
            
            if (statusDto.Status == "InProgress" && !ride.PickupTime.HasValue)
            {
                ride.PickupTime = DateTime.UtcNow;
            }
            else if (statusDto.Status == "Completed" && !ride.DropoffTime.HasValue)
            {
                ride.DropoffTime = DateTime.UtcNow;
                
                // Update rider stats
                var rider = _context.Riders.FirstOrDefault(r => r.id == ride.RiderId);
                if (rider != null)
                {
                    rider.totalRides++;
                    
                    if (ride.IsPaid)
                    {
                        rider.totalEarnings += ride.Price;
                    }
                }
            }
            
            var saveResult = _context.SaveChanges();
            if (saveResult > 0)
            {
                return Ok($"Ride status updated from {oldStatus} to {statusDto.Status}");
            }
            
            return Problem("Failed to save changes to database");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("location/update")]
    public IActionResult UpdateLocation([FromForm] int riderId, [FromForm] double latitude, [FromForm] double longitude)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == riderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        rider.latitude = latitude;
        rider.longitude = longitude;
        rider.lastLocationUpdate = DateTime.UtcNow;
        
        if (_context.SaveChanges() > 0)
            return Ok("Location updated successfully");
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpPost("document/upload")]
    public IActionResult UploadDocument([FromForm] DriverDocumentDto documentDto)
    {
        var rider = _context.Riders.FirstOrDefault(r => r.id == documentDto.RiderId);
        if (rider == null)
            return NotFound("Rider not found");
        
        var document = new DriverDocument
        {
            RiderId = documentDto.RiderId,
            DocumentType = documentDto.DocumentType,
            DocumentNumber = documentDto.DocumentNumber,
            ExpirationDate = documentDto.ExpirationDate,
            Status = "Pending"
        };
        
        _context.DriverDocuments.Add(document);
        
        if (_context.SaveChanges() > 0)
            return Ok($"Document uploaded successfully. Pending verification.");
        
        return Problem("Something went wrong, try again later.");
    }
}