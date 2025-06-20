using Microsoft.EntityFrameworkCore;

namespace TaxiRideAPI.Models;

public class TaxiRideDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Rider> Riders { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<Ride> RideHistory { get; set; }
    public DbSet<RideOffer> RideOffers { get; set; }
    public DbSet<TaxiCompany> TaxiCompanies { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<DriverDocument> DriverDocuments { get; set; }
    public DbSet<DriverAvailability> DriverAvailabilities { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<FinancialReport> FinancialReports { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<RideReservation> RideReservations { get; set; }
    public DbSet<ComparisonRequest> ComparisonRequests { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    public TaxiRideDbContext(DbContextOptions<TaxiRideDbContext> options) : base(options)
    {
        
    }
}