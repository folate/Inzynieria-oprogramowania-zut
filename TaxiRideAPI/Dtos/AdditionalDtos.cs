using System.ComponentModel.DataAnnotations;

namespace TaxiRideAPI.Dtos;

public class OfferComparisonDto
{
    public int UserId { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string? VehicleTypeFilter { get; set; }
    public string? SortBy { get; set; } = "Price"; // Price, Time, Rating
    public bool ComfortOnly { get; set; } = false;
}

public class OfferResponseDto
{
    public int Id { get; set; }
    public int RiderId { get; set; }
    public string? RiderName { get; set; }
    public double Price { get; set; }
    public int EstimatedTime { get; set; }
    public string? VehicleType { get; set; }
    public double DriverRating { get; set; }
    public string? CompanyName { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public bool IsAvailable { get; set; }
}

public class RideStatusDto
{
    public int RideId { get; set; }
    public required string Status { get; set; }
    public int? RiderId { get; set; }
    public string? RiderName { get; set; }
    public string? RiderPhone { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public int? EstimatedArrival { get; set; }
    public double? CurrentPrice { get; set; }
}

public class FinancialReportRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string ReportType { get; set; }
    public bool IncludeVat { get; set; } = false;
    public string? ExportFormat { get; set; } = "PDF";
}

public class SupportTicketCreateDto
{
    public int UserId { get; set; }
    public int? RideId { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }
    public string Priority { get; set; } = "Medium";
}

public class SupportTicketUpdateDto
{
    public int TicketId { get; set; }
    public required string Status { get; set; }
    public string? Resolution { get; set; }
    public double? RefundAmount { get; set; }
    public int? AssignedToWorkerId { get; set; }
}

public class TicketMessageDto
{
    public int TicketId { get; set; }
    public int SenderId { get; set; }
    public required string Message { get; set; }
    public bool IsFromSupport { get; set; }
}

public class TicketMessageWithIdDto
{
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public required string Message { get; set; }
}

public class RideAcceptDto
{
    public int RideId { get; set; }
    public int RiderId { get; set; }
}

public class LocationUpdateDto
{
    public int RiderId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RideCompleteDto
{
    public int RideId { get; set; }
    public double FinalPrice { get; set; }
    public required string PaymentMethod { get; set; }
    public double? Distance { get; set; }
    public int? Duration { get; set; }
}
