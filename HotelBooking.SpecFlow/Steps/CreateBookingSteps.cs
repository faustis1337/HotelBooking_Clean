using HotelBooking.Core;
using HotelBooking.Core.BindingModels;
using HotelBooking.Core.Entities;
using HotelBooking.Core.Exceptions;
using HotelBooking.Core.Interfaces;
using HotelBooking.Core.Services;
using Moq;
namespace HotelBooking.SpecFlow.Steps;

[Binding]
public class CreateBookingSteps {
    private readonly IBookingManager bookingManager;
    private readonly Mock<IRepository<Booking>> bookingRepository;
    private readonly Mock<IRepository<Room>> roomRepository;
    private readonly Mock<IRepository<Customer>> customerRepository;
    
    public CreateBookingSteps()
    {
        bookingRepository = new Mock<IRepository<Booking>>();
        roomRepository = new Mock<IRepository<Room>>();
        customerRepository = new Mock<IRepository<Customer>>();
        
        bookingManager = new BookingManager(bookingRepository.Object,roomRepository.Object,customerRepository.Object);
    }

    #region Setup
    [BeforeScenario]
    public void SetupMocks()
    {
        // Inject Rooms 
        List<Room> rooms = new() {
            new Room { Id=1, Description="A" },
            new Room { Id=2, Description="B" },
        };
        
        roomRepository.Setup(x => x.GetAll()).Returns(rooms);
        
        // Inject Customers 
        List<Customer> customers = new ()
        {
            new Customer { Id=1, Name= "Bo Benson" , Email = "BB@mail.com"},
            new Customer { Id=2, Name= "Joe Johnson" , Email = "JoJo@mail.com"},
            new Customer { Id=3, Name= "Steve Stevenson" , Email = "stoo@mail.com"},
        };
        
        customerRepository.Setup(x => x.GetAll()).Returns(customers);
        customerRepository.Setup(x => x.Get(1)).Returns(customers[0]);
        customerRepository.Setup(x => x.Get(2)).Returns(customers[1]);
        customerRepository.Setup(x => x.Get(3)).Returns(customers[2]);
    }
    #endregion

    #region Arrange
    
    [Given(@"there are no fully booked dates")]
    public void GivenThereAreNoFullyBookedDates()
    {
        bookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking>());
    }
    
    [Given(@"Fully booked date period starts in (.*) days and ends in (.*) days")]
    public void GivenFullyBookedDatePeriodStartsInDaysAndEndsInDays(int p0, int p1)
    {
        bookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking> {
            new() {
                CustomerId = 1,
                StartDate = DateTime.Today.AddDays(p0),
                EndDate = DateTime.Today.AddDays(p1),
                RoomId = 1,
                IsActive = true
            },
            new() {
                CustomerId = 2,
                StartDate = DateTime.Today.AddDays(p0),
                EndDate = DateTime.Today.AddDays(p1),
                RoomId = 2,
                IsActive = true
            }
        });
    }
    
    #endregion
    
    #region Act
    
    [When(@"the user makes a booking that starts in (.*) days and ends in (.*) days")]
    public void WhenTheUserMakesABookingThatStartsInDaysAndEndsInDays(int p0, int p1)
    {
        try {
            bookingManager.CreateBooking(new BookingPostBindingModel {
                StartDate = DateTime.Today.AddDays(p0),
                EndDate = DateTime.Today.AddDays(p1),
                CustomerId = 3
            });
        } catch (RestException e) { // When a restException is thrown, it means the user did an invalid action and was notified via a Toast in the UI
        }
        
    }
    
    #endregion
    
    #region Assert
    [Then(@"the booking is created")]
    public void ThenTheBookingIsCreated()
    {
        bookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()),Times.Exactly(1));
    }
    
    [Then(@"the booking is not created")]
    public void ThenTheBookingIsNotCreated()
    {
        bookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()),Times.Exactly(0));
    }
    
    [Then(@"the booking is created: true")]
    public void ThenTheBookingIsCreatedTrue()
    {
        bookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()),Times.Exactly(1));
    }
    
    [Then(@"the booking is created: false")]
    public void ThenTheBookingIsCreatedFalse()
    {
        bookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()),Times.Exactly(0));
    }
    #endregion
}