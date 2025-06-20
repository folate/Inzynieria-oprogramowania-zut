namespace TaxiRideAPI.Dtos;

public class RiderDto
{
    public int Id { get; set; }
    public required string Login { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public double StarRating { get; set; }
}