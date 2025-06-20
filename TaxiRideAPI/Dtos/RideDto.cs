namespace TaxiRideAPI.Dtos;

public class RideHistoryDto
{
    public int Id { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public DateTime OrderTime { get; set; }
    public DateTime? PickupTime { get; set; }
    public DateTime? DropoffTime { get; set; }
    public double Price { get; set; }
    public required string Status { get; set; }
    public int? Rating { get; set; }
    public required string RiderName { get; set; }
    public bool IsPaid { get; set; }
    public required string PaymentMethod { get; set; }
}

public class RideRatingDto
{
    public int RideId { get; set; }
    public int Rating { get; set; }
    public required string Comment { get; set; }
}

public class RideStatusUpdateDto
{
    public int RideId { get; set; }
    public required string Status { get; set; }
}

public class RidePaymentDto
{
    public int RideId { get; set; }
    public required string PaymentMethod { get; set; }
    public double Amount { get; set; }
}

public class ComplaintDto
{
    public int RideId { get; set; }
    public required string Description { get; set; }
}

public class DriverAvailabilityDto
{
    public int RiderId { get; set; }
    public bool IsAvailable { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class DriverDocumentDto
{
    public int RiderId { get; set; }
    public required string DocumentType { get; set; }
    public required string DocumentNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class CompanyStatisticsDto
{
    public int TotalRiders { get; set; }
    public int TotalRides { get; set; }
    public double TotalRevenue { get; set; }
    public int PendingRides { get; set; }
    public int CompletedRides { get; set; }
    public int CancelledRides { get; set; }
}

public class PromotionDto
{
    public required string Code { get; set; }
    public required string Description { get; set; }
    public double DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
