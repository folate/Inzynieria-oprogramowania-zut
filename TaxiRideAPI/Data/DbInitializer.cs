using TaxiRideAPI.Models;

namespace TaxiRideAPI.Data;

public static class DbInitializer
{
    public static void Initialize(TaxiRideDbContext context)
    {
        context.Database.EnsureCreated();

        // Check if we need to seed basic data
        bool needsBasicSeeding = !context.Users.Any();
        
        // Check if we need to seed owners and workers
        bool needsOwnerSeeding = !context.Owners.Any();
        bool needsWorkerSeeding = !context.Workers.Any();

        if (needsBasicSeeding)
        {
            // Seed Users
            var users = new User[]
            {
                new User
                {
                    login = "john.doe",
                    password = "password123",
                    email = "john.doe@example.com",
                    phoneNumber = "555-123-4567",
                    firstName = "John",
                    lastName = "Doe",
                    preferredPaymentMethod = "Credit Card"
                },
                new User
                {
                    login = "jane.smith",
                    password = "password123",
                    email = "jane.smith@example.com",
                    phoneNumber = "555-987-6543",
                    firstName = "Jane",
                    lastName = "Smith",
                    preferredPaymentMethod = "PayPal"
                },
                new User
                {
                    login = "robert.johnson",
                    password = "password123",
                    email = "robert.johnson@example.com",
                    phoneNumber = "555-555-5555",
                    firstName = "Robert",
                    lastName = "Johnson",
                    preferredPaymentMethod = "Cash"
                }
            };

            foreach (var user in users)
            {
                context.Users.Add(user);
            }
            context.SaveChanges();

            // Seed Taxi Companies
            var companies = new TaxiCompany[]
            {
                new TaxiCompany
                {
                    Name = "Express Taxi",
                    PhoneNumber = "555-100-2000",
                    Email = "info@expresstaxi.com",
                    Address = "123 Main St, Cityville"
                },
                new TaxiCompany
                {
                    Name = "City Cabs",
                    PhoneNumber = "555-200-3000",
                    Email = "info@citycabs.com",
                    Address = "456 Elm St, Townsburg"
                },
                new TaxiCompany
                {
                    Name = "Premium Rides",
                    PhoneNumber = "555-300-4000",
                    Email = "info@premiumrides.com",
                    Address = "789 Oak St, Metropolis"
                }
            };

            foreach (var company in companies)
            {
                context.TaxiCompanies.Add(company);
            }
            context.SaveChanges();

            // Seed Riders (Drivers)
            var riders = new Rider[]
            {
                new Rider
                {
                    login = "driver1",
                    password = "password123",
                    name = "Michael",
                    surname = "Brown",
                    phoneNumber = "555-111-2222",
                    email = "michael.brown@example.com",
                    starRating = 4.8,
                    companyId = 1,
                    isAvailable = true,
                    latitude = 52.2297,
                    longitude = 21.0122,
                    lastLocationUpdate = DateTime.UtcNow,
                    totalRides = 142,
                    totalEarnings = 3245.50,
                    licenseNumber = "DL123456789",
                    licenseExpiryDate = DateTime.UtcNow.AddYears(2),
                    isVerified = true,
                    vehicleInfo = "Toyota Camry, Białe"
                },
                new Rider
                {
                    login = "driver2",
                    password = "password123",
                    name = "David",
                    surname = "Wilson",
                    phoneNumber = "555-222-3333",
                    email = "david.wilson@example.com",
                    starRating = 4.5,
                    companyId = 1,
                    isAvailable = true,
                    latitude = 52.2360,
                    longitude = 21.0193,
                    lastLocationUpdate = DateTime.UtcNow,
                    totalRides = 98,
                    totalEarnings = 2145.75,
                    licenseNumber = "DL987654321",
                    licenseExpiryDate = DateTime.UtcNow.AddYears(3),
                    isVerified = true,
                    vehicleInfo = "Honda Accord, Czarne"
                },
                new Rider
                {
                    login = "driver3",
                    password = "password123",
                    name = "Elizabeth",
                    surname = "Taylor",
                    phoneNumber = "555-333-4444",
                    email = "elizabeth.taylor@example.com",
                    starRating = 4.9,
                    companyId = 2,
                    isAvailable = true,
                    latitude = 52.2322,
                    longitude = 21.0068,
                    lastLocationUpdate = DateTime.UtcNow,
                    totalRides = 215,
                    totalEarnings = 4320.25,
                    licenseNumber = "DL456789123",
                    licenseExpiryDate = DateTime.UtcNow.AddYears(1),
                    isVerified = true,
                    vehicleInfo = "BMW 3 Series, Srebrne"
                },
                new Rider
                {
                    login = "driver4",
                    password = "password123",
                    name = "Thomas",
                    surname = "Anderson",
                    phoneNumber = "555-444-5555",
                    email = "thomas.anderson@example.com",
                    starRating = 4.2,
                    companyId = 3,
                    isAvailable = false,
                    latitude = 52.2389,
                    longitude = 21.0111,
                    lastLocationUpdate = DateTime.UtcNow,
                    totalRides = 67,
                    totalEarnings = 1345.50,
                    licenseNumber = "DL789123456",
                    licenseExpiryDate = DateTime.UtcNow.AddYears(2),
                    isVerified = true,
                    vehicleInfo = "Mercedes E-Class, Niebieskie"
                },
                new Rider
                {
                    login = "driver5",
                    password = "password123",
                    name = "Sarah",
                    surname = "Johnson",
                    phoneNumber = "555-555-6666",
                    email = "sarah.johnson@example.com",
                    starRating = 4.7,
                    companyId = 2,
                    isAvailable = true,
                    latitude = 52.2330,
                    longitude = 21.0151,
                    lastLocationUpdate = DateTime.UtcNow,
                    totalRides = 178,
                    totalEarnings = 3675.25,
                    licenseNumber = "DL321654987",
                    licenseExpiryDate = DateTime.UtcNow.AddYears(4),
                    isVerified = true,
                    vehicleInfo = "Audi A4, Czerwone"
                }
            };

            foreach (var rider in riders)
            {
                context.Riders.Add(rider);
            }
            context.SaveChanges();

            // Seed Driver Documents
            var documents = new DriverDocument[]
            {
                new DriverDocument
                {
                    RiderId = 1,
                    DocumentType = "Driver License",
                    DocumentNumber = "DL123456",
                    ExpirationDate = DateTime.UtcNow.AddYears(3),
                    Status = "Valid"
                },
                new DriverDocument
                {
                    RiderId = 1,
                    DocumentType = "Insurance",
                    DocumentNumber = "INS789012",
                    ExpirationDate = DateTime.UtcNow.AddMonths(6),
                    Status = "Valid"
                },
                new DriverDocument
                {
                    RiderId = 2,
                    DocumentType = "Driver License",
                    DocumentNumber = "DL234567",
                    ExpirationDate = DateTime.UtcNow.AddYears(2),
                    Status = "Valid"
                },
                new DriverDocument
                {
                    RiderId = 3,
                    DocumentType = "Driver License",
                    DocumentNumber = "DL345678",
                    ExpirationDate = DateTime.UtcNow.AddMonths(1),
                    Status = "Expiring Soon"
                },
                new DriverDocument
                {
                    RiderId = 4,
                    DocumentType = "Driver License",
                    DocumentNumber = "DL456789",
                    ExpirationDate = DateTime.UtcNow.AddYears(4),
                    Status = "Valid"
                },
                new DriverDocument
                {
                    RiderId = 5,
                    DocumentType = "Driver License",
                    DocumentNumber = "DL567890",
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    Status = "Valid"
                }
            };

            foreach (var document in documents)
            {
                context.DriverDocuments.Add(document);
            }
            context.SaveChanges();

            // Seed Driver Availability
            var availabilities = new DriverAvailability[]
            {
                new DriverAvailability
                {
                    RiderId = 1,
                    StartTime = DateTime.UtcNow.AddHours(-4),
                    EndTime = DateTime.UtcNow.AddHours(4),
                    IsAvailable = true
                },
                new DriverAvailability
                {
                    RiderId = 2,
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    EndTime = DateTime.UtcNow.AddHours(6),
                    IsAvailable = true
                },
                new DriverAvailability
                {
                    RiderId = 3,
                    StartTime = DateTime.UtcNow.AddHours(-5),
                    EndTime = DateTime.UtcNow.AddHours(3),
                    IsAvailable = true
                },
                new DriverAvailability
                {
                    RiderId = 5,
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    EndTime = DateTime.UtcNow.AddHours(7),
                    IsAvailable = true
                }
            };

            foreach (var availability in availabilities)
            {
                context.DriverAvailabilities.Add(availability);
            }
            context.SaveChanges();

            // Seed Promotions
            var promotions = new Promotion[]
            {
                new Promotion
                {
                    Code = "WELCOME20",
                    Description = "20% off your first ride",
                    DiscountPercentage = 20,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(60),
                    IsActive = true
                },
                new Promotion
                {
                    Code = "SUMMER25",
                    Description = "25% off rides during summer",
                    DiscountPercentage = 25,
                    StartDate = DateTime.UtcNow.AddDays(-15),
                    EndDate = DateTime.UtcNow.AddDays(75),
                    IsActive = true
                },
                new Promotion
                {
                    Code = "WEEKEND10",
                    Description = "10% off all weekend rides",
                    DiscountPercentage = 10,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(180),
                    IsActive = true
                }
            };

            foreach (var promotion in promotions)
            {
                context.Promotions.Add(promotion);
            }
            context.SaveChanges();

            // Seed Rides with History
            var rides = new Ride[]
            {
                new Ride
                {
                    UserId = 1,
                    RiderId = 1,
                    PickupLocation = "123 Apple St, Cityville",
                    DropoffLocation = "456 Orange Ave, Cityville",
                    OrderTime = DateTime.UtcNow.AddDays(-5),
                    PickupTime = DateTime.UtcNow.AddDays(-5).AddMinutes(10),
                    DropoffTime = DateTime.UtcNow.AddDays(-5).AddMinutes(30),
                    Price = 25.50,
                    Status = "Completed",
                    Rating = 5,
                    Comment = "Great driver, very punctual!",
                    IsPaid = true,
                    PaymentMethod = "Credit Card"
                },
                new Ride
                {
                    UserId = 1,
                    RiderId = 3,
                    PickupLocation = "789 Grape St, Cityville",
                    DropoffLocation = "101 Banana Blvd, Cityville",
                    OrderTime = DateTime.UtcNow.AddDays(-3),
                    PickupTime = DateTime.UtcNow.AddDays(-3).AddMinutes(12),
                    DropoffTime = DateTime.UtcNow.AddDays(-3).AddMinutes(45),
                    Price = 32.75,
                    Status = "Completed",
                    Rating = 4,
                    Comment = "Good ride, but traffic was heavy.",
                    IsPaid = true,
                    PaymentMethod = "PayPal"
                },
                new Ride
                {
                    UserId = 2,
                    RiderId = 2,
                    PickupLocation = "222 Pine St, Townsburg",
                    DropoffLocation = "333 Elm Ave, Townsburg",
                    OrderTime = DateTime.UtcNow.AddDays(-2),
                    PickupTime = DateTime.UtcNow.AddDays(-2).AddMinutes(8),
                    DropoffTime = DateTime.UtcNow.AddDays(-2).AddMinutes(22),
                    Price = 18.25,
                    Status = "Completed",
                    Rating = 5,
                    Comment = "Perfect ride, clean car!",
                    IsPaid = true,
                    PaymentMethod = "Credit Card"
                },
                new Ride
                {
                    UserId = 3,
                    RiderId = 5,
                    PickupLocation = "444 Oak St, Metropolis",
                    DropoffLocation = "555 Maple Dr, Metropolis",
                    OrderTime = DateTime.UtcNow.AddDays(-1),
                    PickupTime = DateTime.UtcNow.AddDays(-1).AddMinutes(15),
                    DropoffTime = DateTime.UtcNow.AddDays(-1).AddMinutes(35),
                    Price = 22.00,
                    Status = "Completed",
                    Rating = 3,
                    Comment = "Driver was a bit late, but the ride was okay.",
                    IsPaid = true,
                    PaymentMethod = "Cash"
                },
                new Ride
                {
                    UserId = 2,
                    RiderId = 1,
                    PickupLocation = "666 Cherry St, Cityville",
                    DropoffLocation = "777 Pear Ave, Cityville",
                    OrderTime = DateTime.UtcNow.AddHours(-4),
                    PickupTime = DateTime.UtcNow.AddHours(-4).AddMinutes(10),
                    DropoffTime = DateTime.UtcNow.AddHours(-4).AddMinutes(25),
                    Price = 15.75,
                    Status = "Completed",
                    Rating = 5,
                    Comment = "Excellent service!",
                    IsPaid = true,
                    PaymentMethod = "Credit Card"
                },
                new Ride
                {
                    UserId = 1,
                    RiderId = 3,
                    PickupLocation = "888 Walnut St, Townsburg",
                    DropoffLocation = "999 Chestnut Ave, Townsburg",
                    OrderTime = DateTime.UtcNow.AddHours(-2),
                    PickupTime = DateTime.UtcNow.AddHours(-2).AddMinutes(8),
                    Status = "InProgress",
                    Price = 28.50,
                    IsPaid = false
                },
                new Ride
                {
                    UserId = 3,
                    RiderId = 2,
                    PickupLocation = "123 Main St, Metropolis",
                    DropoffLocation = "456 First Ave, Metropolis",
                    OrderTime = DateTime.UtcNow.AddMinutes(-30),
                    Status = "Accepted",
                    Price = 22.25,
                    IsPaid = false
                },
                new Ride
                {
                    UserId = 2,
                    PickupLocation = "789 Second St, Cityville",
                    DropoffLocation = "101 Third Ave, Cityville",
                    OrderTime = DateTime.UtcNow.AddMinutes(-10),
                    Status = "Pending",
                    Price = 0,
                    IsPaid = false
                }
            };

            foreach (var ride in rides)
            {
                context.RideHistory.Add(ride);
            }
            context.SaveChanges();

            // Seed Ride Offers for pending ride
            var pendingRide = context.RideHistory.FirstOrDefault(r => r.Status == "Pending");
            if (pendingRide != null)
            {
                var offers = new RideOffer[]
                {
                    new RideOffer
                    {
                        RideId = pendingRide.Id,
                        RiderId = 1,
                        Price = 24.50,
                        EstimatedTime = 12,
                        IsAccepted = false,
                        VehicleType = "Standard"
                    },
                    new RideOffer
                    {
                        RideId = pendingRide.Id,
                        RiderId = 3,
                        Price = 22.75,
                        EstimatedTime = 15,
                        IsAccepted = false,
                        VehicleType = "Comfort"
                    },
                    new RideOffer
                    {
                        RideId = pendingRide.Id,
                        RiderId = 5,
                        Price = 26.00,
                        EstimatedTime = 10,
                        IsAccepted = false,
                        VehicleType = "Premium"
                    }
                };

                foreach (var offer in offers)
                {
                    context.RideOffers.Add(offer);
                }
                context.SaveChanges();
            }

            // Seed Complaints
            var complaints = new Complaint[]
            {
                new Complaint
                {
                    UserId = 3,
                    RideId = 4,
                    Description = "Driver was late and the car was not clean.",
                    FiledDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Resolved",
                    Resolution = "Offered a discount coupon for the next ride."
                },
                new Complaint
                {
                    UserId = 1,
                    RideId = 2,
                    Description = "The route taken was much longer than necessary.",
                    FiledDate = DateTime.UtcNow.AddHours(-12),
                    Status = "InReview"
                }
            };

            foreach (var complaint in complaints)
            {
                context.Complaints.Add(complaint);
            }
            context.SaveChanges();
        }

        // Always seed owners and workers if they don't exist
        if (needsOwnerSeeding)
        {
            // Ensure we have companies first
            if (!context.TaxiCompanies.Any())
            {
                var companies = new TaxiCompany[]
                {
                    new TaxiCompany
                    {
                        Name = "Express Taxi",
                        PhoneNumber = "555-100-2000",
                        Email = "info@expresstaxi.com",
                        Address = "123 Main St, Cityville"
                    },
                    new TaxiCompany
                    {
                        Name = "City Cabs",
                        PhoneNumber = "555-200-3000",
                        Email = "info@citycabs.com",
                        Address = "456 Elm St, Townsburg"
                    },
                    new TaxiCompany
                    {
                        Name = "Premium Rides",
                        PhoneNumber = "555-300-4000",
                        Email = "info@premiumrides.com",
                        Address = "789 Oak St, Metropolis"
                    }
                };

                foreach (var company in companies)
                {
                    context.TaxiCompanies.Add(company);
                }
                context.SaveChanges();
            }

            // Seed Owners
            var owners = new Owner[]
            {
                new Owner
                {
                    login = "owner1",
                    password = "password123",
                    name = "Jan",
                    surname = "Kowalski",
                    phoneNumber = "+48 123 456 789",
                    email = "jan.kowalski@taxiride.com",
                    companyId = 1
                },
                new Owner
                {
                    login = "owner2",
                    password = "password123",
                    name = "Anna",
                    surname = "Nowak",
                    phoneNumber = "+48 987 654 321",
                    email = "anna.nowak@taxiride.com",
                    companyId = 2
                },
                new Owner
                {
                    login = "owner3",
                    password = "password123",
                    name = "Piotr",
                    surname = "Wiśniewski",
                    phoneNumber = "+48 555 666 777",
                    email = "piotr.wisniewski@taxiride.com",
                    companyId = 3
                }
            };

            foreach (var owner in owners)
            {
                context.Owners.Add(owner);
            }
            context.SaveChanges();
        }

        if (needsWorkerSeeding)
        {
            // Seed Workers
            var workers = new Worker[]
            {
                new Worker
                {
                    login = "worker1",
                    password = "password123",
                    name = "Marta",
                    surname = "Zielińska",
                    phoneNumber = "+48 111 222 333",
                    email = "marta.zielinska@taxiride.com",
                    department = "Support",
                    isActive = true
                },
                new Worker
                {
                    login = "worker2",
                    password = "password123",
                    name = "Tomasz",
                    surname = "Lewandowski",
                    phoneNumber = "+48 444 555 666",
                    email = "tomasz.lewandowski@taxiride.com",
                    department = "Finance",
                    isActive = true
                },
                new Worker
                {
                    login = "worker3",
                    password = "password123",
                    name = "Katarzyna",
                    surname = "Dąbrowska",
                    phoneNumber = "+48 777 888 999",
                    email = "katarzyna.dabrowska@taxiride.com",
                    department = "Operations",
                    isActive = true
                },
                new Worker
                {
                    login = "worker4",
                    password = "password123",
                    name = "Michał",
                    surname = "Kaczmarek",
                    phoneNumber = "+48 123 789 456",
                    email = "michal.kaczmarek@taxiride.com",
                    department = "Support",
                    isActive = true
                },
                new Worker
                {
                    login = "worker5",
                    password = "password123",
                    name = "Agnieszka",
                    surname = "Pawlak",
                    phoneNumber = "+48 456 123 789",
                    email = "agnieszka.pawlak@taxiride.com",
                    department = "Finance",
                    isActive = true
                }
            };

            foreach (var worker in workers)
            {
                context.Workers.Add(worker);
            }
            context.SaveChanges();

            // Seed Support Tickets for workers to manage (only if we have users)
            if (context.Users.Any() && !context.SupportTickets.Any())
            {
                var tickets = new SupportTicket[]
                {
                    new SupportTicket
                    {
                        TicketNumber = "TICKET-001",
                        UserId = 1,
                        Subject = "Problemy z płatnością",
                        Description = "Nie mogę zapłacić za przejazd kartą kredytową. System pokazuje błąd.",
                        Status = "New",
                        Priority = "Medium",
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        AssignedToWorkerId = 1 // Worker ID 1 (Marta Zielińska)
                    },
                    new SupportTicket
                    {
                        TicketNumber = "TICKET-002",
                        UserId = 2,
                        Subject = "Kierowca się nie pojawił",
                        Description = "Zamówiłem przejazd, ale kierowca nie przyjechał w umówionym czasie.",
                        Status = "InProgress",
                        Priority = "High",
                        CreatedDate = DateTime.UtcNow.AddDays(-1),
                        AssignedToWorkerId = 1 // Worker ID 1 (Marta Zielińska)
                    },
                    new SupportTicket
                    {
                        TicketNumber = "TICKET-003",
                        UserId = 3,
                        Subject = "Zwrot pieniędzy",
                        Description = "Chciałbym zwrot pieniędzy za przejazd, który został anulowany.",
                        Status = "New",
                        Priority = "Medium",
                        CreatedDate = DateTime.UtcNow.AddHours(-6),
                        AssignedToWorkerId = 2 // Worker ID 2 (Tomasz Lewandowski)
                    },
                    new SupportTicket
                    {
                        TicketNumber = "TICKET-004",
                        UserId = 1,
                        Subject = "Aplikacja się zawiesza",
                        Description = "Aplikacja mobilna zawiesza się podczas zamawiania przejazdu.",
                        Status = "New",
                        Priority = "High",
                        CreatedDate = DateTime.UtcNow.AddHours(-3),
                        AssignedToWorkerId = 4 // Worker ID 4 (Michał Kaczmarek)
                    },
                    new SupportTicket
                    {
                        TicketNumber = "TICKET-005",
                        UserId = 2,
                        Subject = "Błędne naliczenie opłaty",
                        Description = "Zostałem obciążony wyższą kwotą niż powinienem.",
                        Status = "Resolved",
                        Priority = "High",
                        CreatedDate = DateTime.UtcNow.AddDays(-3),
                        ResolvedDate = DateTime.UtcNow.AddDays(-1),
                        AssignedToWorkerId = 2, // Worker ID 2 (Tomasz Lewandowski)
                        Resolution = "Zwrot nadpłaty został przetworzony"
                    }
                };

                foreach (var ticket in tickets)
                {
                    context.SupportTickets.Add(ticket);
                }
                context.SaveChanges();

                // Seed Ticket Messages
                var messages = new TicketMessage[]
                {
                    new TicketMessage
                    {
                        TicketId = 2,
                        SenderId = 1, // Worker ID 1 (Marta Zielińska)
                        Message = "Przepraszamy za niedogodności. Sprawdzamy sytuację z kierowcą.",
                        SentDate = DateTime.UtcNow.AddHours(-12),
                        IsFromSupport = true
                    },
                    new TicketMessage
                    {
                        TicketId = 2,
                        SenderId = 1, // Worker ID 1 (Marta Zielińska)
                        Message = "Kierowca został ukarany. Otrzymasz kupon zniżkowy na następny przejazd.",
                        SentDate = DateTime.UtcNow.AddHours(-6),
                        IsFromSupport = true
                    },
                    new TicketMessage
                    {
                        TicketId = 5,
                        SenderId = 2, // Worker ID 2 (Tomasz Lewandowski)
                        Message = "Zwrot został przetworzony. Pieniądze będą widoczne na koncie w ciągu 3-5 dni roboczych.",
                        SentDate = DateTime.UtcNow.AddDays(-1),
                        IsFromSupport = true
                    }
                };

                foreach (var message in messages)
                {
                    context.TicketMessages.Add(message);
                }
                context.SaveChanges();
            }
        }
    }
}
