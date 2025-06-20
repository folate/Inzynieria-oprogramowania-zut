using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxiRideAPI.Dtos;
using TaxiRideAPI.Models;

namespace TaxiRideAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private TaxiRideDbContext _context;
    public UserController(TaxiRideDbContext dbContext)
    {
        _context = dbContext;
    }
    
    [HttpGet("")]
    public IActionResult Get()
    {
        return Ok("TaxiRide API - User Service");
    }

    [HttpPost("login")]
    public IActionResult Login([FromForm] string login, [FromForm] string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.login == login);
        if (user != null && user.password == password)
        {
            return Ok(new { message = $"Hello {user.login}!", userId = user.id });
        }
        return Unauthorized("unauthorized");
    }

    [HttpPost("register")]
    public IActionResult Register([FromForm] string username, [FromForm] string password, [FromForm] string email, [FromForm] string phoneNumber)
    {
        if (username == "admin" || _context.Users.Any(u => u.login == username)) 
            return BadRequest("Username is taken");
        
        var user = new User { 
            login = username, 
            password = password,
            email = email ?? string.Empty,
            phoneNumber = phoneNumber ?? string.Empty
        };
        
        _context.Users.Add(user);
        if (_context.SaveChanges() > 0)
            return Ok(new { message = $"Registered {user.login}!", userId = user.id });
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpGet("profile/{id}")]
    public IActionResult GetProfile(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.id == id);
        if (user == null)
            return NotFound("User not found");
        
        return Ok(new {
            id = user.id,
            login = user.login,
            email = user.email,
            phoneNumber = user.phoneNumber,
            firstName = user.firstName,
            lastName = user.lastName,
            preferredPaymentMethod = user.preferredPaymentMethod
        });
    }
    
    [HttpPost("profile/update")]
    public IActionResult UpdateProfile([FromForm] int id, [FromForm] string firstName, [FromForm] string lastName, 
                                        [FromForm] string email, [FromForm] string phoneNumber, [FromForm] string preferredPaymentMethod)
    {
        var user = _context.Users.FirstOrDefault(u => u.id == id);
        if (user == null)
            return NotFound("User not found");
        
        user.firstName = firstName ?? user.firstName;
        user.lastName = lastName ?? user.lastName;
        user.email = email ?? user.email;
        user.phoneNumber = phoneNumber ?? user.phoneNumber;
        user.preferredPaymentMethod = preferredPaymentMethod ?? user.preferredPaymentMethod;
        
        if (_context.SaveChanges() > 0)
            return Ok("Profile updated successfully");
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpGet("rides/{userId}")]
    public IActionResult GetRideHistory(int userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.id == userId);
        if (user == null)
            return NotFound("User not found");
        
        var rides = _context.RideHistory
            .Include(r => r.Rider)
            .Where(r => r.UserId == userId)
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
            RiderName = (r.Rider != null && r.Rider.name != null && r.Rider.surname != null) ? $"{r.Rider.name} {r.Rider.surname}" : "N/A",
            IsPaid = r.IsPaid,
            PaymentMethod = r.PaymentMethod
        }).ToList();
        
        return Ok(rideHistoryDtos);
    }
    
    [HttpPost("ride/rate")]
    public IActionResult RateRide([FromForm] RideRatingDto ratingDto)
    {
        var ride = _context.RideHistory.FirstOrDefault(r => r.Id == ratingDto.RideId);
        if (ride == null)
            return NotFound("Ride not found");
        
        if (ride.Status != "Completed")
            return BadRequest("Can only rate completed rides");
        
        ride.Rating = ratingDto.Rating;
        ride.Comment = ratingDto.Comment;
        
        // Update the driver's rating
        var rider = _context.Riders.FirstOrDefault(d => d.id == ride.RiderId);
        if (rider != null)
        {
            var riderRides = _context.RideHistory
                .Where(r => r.RiderId == rider.id && r.Rating.HasValue)
                .ToList();
            
            rider.starRating = riderRides.Count > 0 
                ? riderRides.Average(r => r.Rating.Value) 
                : rider.starRating;
            
            rider.totalRides = _context.RideHistory.Count(r => r.RiderId == rider.id && r.Status == "Completed");
        }
        
        if (_context.SaveChanges() > 0)
            return Ok("Rating submitted successfully");
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpPost("complaint/file")]
    public IActionResult FileComplaint([FromForm] ComplaintDto complaintDto)
    {
        var ride = _context.RideHistory.FirstOrDefault(r => r.Id == complaintDto.RideId);
        if (ride == null)
            return NotFound("Ride not found");
        
        var complaint = new Complaint
        {
            UserId = ride.UserId,
            RideId = ride.Id,
            Description = complaintDto.Description,
            FiledDate = DateTime.UtcNow,
            Status = "New"
        };
        
        _context.Complaints.Add(complaint);
        
        if (_context.SaveChanges() > 0)
            return Ok("Complaint filed successfully");
        
        return Problem("Something went wrong, try again later.");
    }
    
    [HttpPost("payment/process")]
    public IActionResult ProcessPayment([FromForm] RidePaymentDto paymentDto)
    {
        var ride = _context.RideHistory.FirstOrDefault(r => r.Id == paymentDto.RideId);
        if (ride == null)
            return NotFound("Ride not found");
        
        if (ride.IsPaid)
            return BadRequest("Ride has already been paid for");
        
        // In a real app, this would integrate with a payment gateway
        ride.IsPaid = true;
        ride.PaymentMethod = paymentDto.PaymentMethod;
        
        // Update driver earnings
        var rider = _context.Riders.FirstOrDefault(d => d.id == ride.RiderId);
        if (rider != null)
        {
            rider.totalEarnings += ride.Price;
        }
        
        if (_context.SaveChanges() > 0)
            return Ok("Payment processed successfully");
        
        return Problem("Something went wrong, try again later.");
    }
}