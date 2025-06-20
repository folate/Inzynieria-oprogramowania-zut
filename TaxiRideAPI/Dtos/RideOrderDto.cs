using System.ComponentModel.DataAnnotations;

namespace TaxiRideAPI.Dtos;

public class RideOrderDto
{
    public int RiderId { get; set; }
    public int UserID { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
}

public class RideCreateDto
{
    public int UserId { get; set; }
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public double? DropoffLatitude { get; set; }
    public double? DropoffLongitude { get; set; }
    public string? VehicleTypePreference { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

public class RideOfferDto
{
    public int RideId { get; set; }
    public int RiderId { get; set; }
    public string? RiderName { get; set; }
    public double RiderRating { get; set; }
    public double Price { get; set; }
    public int EstimatedTime { get; set; }
}

public class AddressValidationDto
{
    public required string Address { get; set; }
    public string? CountryCode { get; set; } = "PL";
}

public class AddressValidationResponseDto
{
    public bool Valid { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Message { get; set; }
}

public class AddressSuggestionDto
{
    public required string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}