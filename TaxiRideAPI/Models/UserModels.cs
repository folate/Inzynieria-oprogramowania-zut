using System.ComponentModel.DataAnnotations;

namespace TaxiRideAPI.Models;

public class User
{
    [Key]
    public int id { get; set; }
    public required string login { get; set; }
    public required string password { get; set; }
    public string email { get; set; } = string.Empty;
    public string phoneNumber { get; set; } = string.Empty;
    public string firstName { get; set; } = string.Empty;
    public string lastName { get; set; } = string.Empty;
    public string preferredPaymentMethod { get; set; } = string.Empty;
    public DateTime registrationDate { get; set; } = DateTime.UtcNow;
    public bool isActive { get; set; } = true;
    public double? latitude { get; set; }
    public double? longitude { get; set; }
    public DateTime? lastLocationUpdate { get; set; }
    public string role { get; set; } = "Customer"; // Customer, Driver, Support, Owner, Worker
}

public class Rider
{
    [Key]
    public int id { get; set; }
    public required string login { get; set; }
    public required string password { get; set; }
    public required string name { get; set; }
    public required string surname { get; set; }
    public required string phoneNumber { get; set; }
    public required string email { get; set; }
    public double starRating { get; set; }
    public int companyId { get; set; } = 0;
    public bool isAvailable { get; set; } = false;
    public double latitude { get; set; } = 0;
    public double longitude { get; set; } = 0;
    public DateTime lastLocationUpdate { get; set; } = DateTime.UtcNow;
    public int totalRides { get; set; } = 0;
    public double totalEarnings { get; set; } = 0;
    public required string licenseNumber { get; set; }
    public DateTime licenseExpiryDate { get; set; }
    public bool isVerified { get; set; } = false;
    public required string vehicleInfo { get; set; } = string.Empty;
    public DateTime lastActiveTime { get; set; } = DateTime.UtcNow;
}

public class Owner
{
    [Key]
    public int id { get; set; }
    public required string login { get; set; }
    public required string password { get; set; }
    public required string name { get; set; }
    public required string surname { get; set; }
    public required string phoneNumber { get; set; }
    public required string email { get; set; }
    public int companyId { get; set; } = 0;
    public bool isActive { get; set; } = true;
    public DateTime registrationDate { get; set; } = DateTime.UtcNow;
    public DateTime lastActiveTime { get; set; } = DateTime.UtcNow;
}

public class Worker
{
    [Key]
    public int id { get; set; }
    public required string login { get; set; }
    public required string password { get; set; }
    public required string name { get; set; }
    public required string surname { get; set; }
    public required string phoneNumber { get; set; }
    public required string email { get; set; }
    public string department { get; set; } = "Support"; // Support, Finance, Operations
    public bool isActive { get; set; } = true;
    public DateTime registrationDate { get; set; } = DateTime.UtcNow;
    public DateTime lastActiveTime { get; set; } = DateTime.UtcNow;
}