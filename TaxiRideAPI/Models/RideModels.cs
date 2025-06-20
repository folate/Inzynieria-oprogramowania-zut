using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxiRideAPI.Models;

public class Ride
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? RiderId { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public DateTime OrderTime { get; set; }
    public DateTime? PickupTime { get; set; }
    public DateTime? DropoffTime { get; set; }
    public double Price { get; set; }
    public required string Status { get; set; } = "Ordered"; // Ordered, Accepted, InProgress, Completed, Cancelled
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public double? DropoffLatitude { get; set; }
    public double? DropoffLongitude { get; set; }
    public double? Distance { get; set; }
    public int? EstimatedDuration { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [ForeignKey("RiderId")]
    public Rider? Rider { get; set; }
}

public class RideOffer
{
    [Key]
    public int Id { get; set; }
    public int RideId { get; set; }
    public int RiderId { get; set; }
    public double Price { get; set; }
    public int EstimatedTime { get; set; } // In minutes
    public bool IsAccepted { get; set; }
    public required string VehicleType { get; set; } = "Standard";
    public double DriverRating { get; set; }
    public int CompanyId { get; set; }
    
    [ForeignKey("RideId")]
    public Ride? Ride { get; set; }
    
    [ForeignKey("RiderId")]
    public Rider? Rider { get; set; }
}

public class TaxiCompany
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
    public double CommissionRate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Complaint
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RideId { get; set; }
    public required string Description { get; set; }
    public DateTime FiledDate { get; set; }
    public required string Status { get; set; } = "New"; // New, InReview, Resolved
    public string? Resolution { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [ForeignKey("RideId")]
    public Ride? Ride { get; set; }
}

public class DriverDocument
{
    [Key]
    public int Id { get; set; }
    public int RiderId { get; set; }
    public required string DocumentType { get; set; } // License, Insurance, etc.
    public required string DocumentNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
    public required string Status { get; set; } = "Pending"; // Valid, Expired, Pending
    
    [ForeignKey("RiderId")]
    public Rider? Rider { get; set; }
}

public class DriverAvailability
{
    [Key]
    public int Id { get; set; }
    public int RiderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    
    [ForeignKey("RiderId")]
    public Rider? Rider { get; set; }
}

public class Promotion
{
    [Key]
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Description { get; set; }
    public double DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class FinancialReport
{
    [Key]
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double TotalRevenue { get; set; }
    public double TotalCommission { get; set; }
    public double TotalVat { get; set; }
    public int TotalRides { get; set; }
    public required string ReportType { get; set; }
    public DateTime GeneratedDate { get; set; }
    public int GeneratedBy { get; set; }
    public required string FilePath { get; set; }
}

public class SupportTicket
{
    [Key]
    public int Id { get; set; }
    public required string TicketNumber { get; set; }
    public int UserId { get; set; }
    public int? RideId { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; } = "New"; // New, InProgress, WaitingForDriver, Resolved, Rejected
    public required string Priority { get; set; } = "Medium"; // Low, Medium, High
    public DateTime CreatedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public int? AssignedToWorkerId { get; set; }
    public string? Resolution { get; set; }
    public double? RefundAmount { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [ForeignKey("RideId")]
    public Ride? Ride { get; set; }
    
    [ForeignKey("AssignedToWorkerId")]
    public Worker? AssignedToWorker { get; set; }
}

public class TicketMessage
{
    [Key]
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int SenderId { get; set; }
    public required string Message { get; set; }
    public DateTime SentDate { get; set; }
    public bool IsFromSupport { get; set; }
    
    [ForeignKey("TicketId")]
    public SupportTicket? Ticket { get; set; }
    
    [ForeignKey("SenderId")]
    public User? Sender { get; set; }
}

public class Vehicle
{
    [Key]
    public int Id { get; set; }
    public int RiderId { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required string Year { get; set; }
    public required string LicensePlate { get; set; }
    public required string Color { get; set; }
    public required string VehicleType { get; set; } = "Standard"; // Standard, Comfort, Premium, Van
    public bool IsActive { get; set; } = true;
    
    [ForeignKey("RiderId")]
    public Rider? Rider { get; set; }
}

public class RideReservation
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedTime { get; set; }
    public required string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Cancelled, Completed
    public int? AssignedRiderId { get; set; }
    public double EstimatedPrice { get; set; }
    public string? VehicleTypePreference { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [ForeignKey("AssignedRiderId")]
    public Rider? AssignedRider { get; set; }
}

public class ComparisonRequest
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string? VehicleTypeFilter { get; set; }
    public string? SortBy { get; set; } // Price, Time, Rating
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
}

public class UserRole
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string RoleName { get; set; } // Customer, Driver, Support, Owner
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
}
