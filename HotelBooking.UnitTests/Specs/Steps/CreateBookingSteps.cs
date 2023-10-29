using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.Core.BindingModels;
using HotelBooking.Core.Entities;
using HotelBooking.Core.Interfaces;
using HotelBooking.Core.Services;
using Moq;
using TechTalk.SpecFlow;
namespace HotelBooking.UnitTests.Specs.Steps;

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
    
    #region Scenario: Booking is before the fully occupied date
    
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
    
    [When(@"The user makes a booking that starts in (.*) day and ends in (.*) days")]
    public void WhenTheUserMakesABookingThatStartsInDayAndEndsInDays(int p0, int p1)
    {
        bookingManager.CreateBooking(new BookingPostBindingModel {
            StartDate = DateTime.Today.AddDays(p0),
            EndDate = DateTime.Today.AddDays(p1),
            CustomerId = 3
        });
    }
    
    [Then(@"the booking is outside of fully occupied dates and is created")]
    public void ThenTheBookingIsOutsideOfFullyOccupiedDatesAndIsCreated()
    {
        bookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()));
    }
    #endregion
}