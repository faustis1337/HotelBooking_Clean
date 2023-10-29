using System;
using System.Collections.Generic;
using System.Linq;
using HotelBooking.Core;
using HotelBooking.Core.BindingModels;
using HotelBooking.Core.Entities;
using HotelBooking.Core.Exceptions;
using HotelBooking.Core.Interfaces;
using HotelBooking.Core.Services;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        private Mock<IRepository<Booking>> bookingRepository;
        private Mock<IRepository<Room>> roomRepository;
        private Mock<IRepository<Customer>> customerRepository;

        public BookingManagerTests()
        {
            customerRepository = new Mock<IRepository<Customer>>();
            bookingRepository = new Mock<IRepository<Booking>>();
            roomRepository = new Mock<IRepository<Room>>();

            var customers = new List<Customer>
            {
                new Customer { Id=1, Name= "Bo Benson" , Email = "BB@mail.com"},
                new Customer { Id=2, Name= "Joe Johnson" , Email = "JoJo@mail.com"},
            };

            var rooms = new List<Room>
            {
                new Room { Id=1, Description="A" },
                new Room { Id=2, Description="B" },
            };

            DateTime fullyOccupiedStartDate = DateTime.Today.AddDays(10);
            DateTime fullyOccupiedEndDate = DateTime.Today.AddDays(20);

            List<Booking> bookings = new List<Booking>
            {
                new Booking { Id=1, StartDate=fullyOccupiedStartDate, EndDate=fullyOccupiedEndDate, IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=2, StartDate=fullyOccupiedStartDate, EndDate=fullyOccupiedEndDate, IsActive=true, CustomerId=2, RoomId=2 },
            };



            roomRepository.Setup(x => x.GetAll()).Returns(rooms);
            bookingRepository.Setup(x => x.GetAll()).Returns(bookings);
            //customerRepository.Setup(x => x.GetAll()).Returns(customers);
            customerRepository.Setup(x => x.Get(It.IsAny<int>())).Returns(customers.FirstOrDefault);
            bookingManager = new BookingManager(bookingRepository.Object, roomRepository.Object, customerRepository.Object);
            }
            #region FindAvailableRoom
            [Fact]
        public void FindAvailableRoom_StartDateNotInTheFuture_ThrowsRestException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Throws<RestException>(act);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public void FindAvailableRoom_StartDateInThePast()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(-5);
            DateTime finishDate = startDate.AddDays(3);
            // Act
            Action act = () => bookingManager.FindAvailableRoom(startDate, finishDate);
            // Assert
            Assert.Throws<RestException>(act);
        }

        [Fact]
        public void FindAvailableRoom_OverlapsBookingButNotAvailableDays()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(6);
            DateTime finishDate = startDate.AddDays(10);
            // Act
            int roomId = bookingManager.FindAvailableRoom(startDate, finishDate);
            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public void FindAvailableRoom_OverlapsBookingPartiallyInclAvailableDays()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime finishDate = startDate.AddDays(10);
            // Act
            int roomId = bookingManager.FindAvailableRoom(startDate, finishDate);
            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public void FindAvailableRoom_OverlapsExistingBookingFullyInclAvailableDays()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime finishDate = startDate.AddDays(35);
            // Act
            int roomId = bookingManager.FindAvailableRoom(startDate, finishDate);
            // Assert
            Assert.Equal(-1, roomId);
        }


        [Fact]
        public void FindAvailableRoom_AfterBooking()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(30);
            DateTime finishDate = startDate.AddDays(5);
            // Act
            int roomId = bookingManager.FindAvailableRoom(startDate, finishDate);
            // Assert
            Assert.NotEqual(-1, roomId);
        }


        #endregion

        #region CreateBooking
        
        [Fact]
        public void BookingManager_CreateBooking_ReturnTrue()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(30);
            DateTime finishDate = startDate.AddDays(5);

            var model = new BookingPostBindingModel
            {
                CustomerId = 1,
                StartDate = startDate,
                EndDate = finishDate,
            };
            //Act
            bool result = bookingManager.CreateBooking(model);
            //Assert
            Assert.True(result);
        }

        [Fact]
        public void BookingManager_CreateBooking_ReturnFalse()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(11);
            DateTime finishDate = DateTime.Today.AddDays(12);
            

            var model = new BookingPostBindingModel
            {
                CustomerId = 1,
                StartDate = startDate,
                EndDate = finishDate,
            };
            //Act
            bool result = bookingManager.CreateBooking(model);
            //Assert
            Assert.False(result);
        }

        #endregion

        #region GetFullyOccupiedDates
        [Fact]
        public void BookingManager_GetFullyOccupiedDates_ThrowArgumentException()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(5);
            DateTime endDate = DateTime.Today.AddDays(2);

            // Act and Assert
            Assert.Throws<HotelBooking.Core.Exceptions.RestException>(() =>
            {
                bookingManager.GetFullyOccupiedDates(startDate, endDate);
            });
        }

        [Fact]
        public void BookingManager_GetFullyOccupiedDates_ReturnsEmptyList()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(21);
            DateTime endDate = DateTime.Today.AddDays(25);
            //Act
            List<DateTime> fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);
            //Assert
            Assert.Empty(fullyOccupiedDates);
        }

        [Fact]
        public void BookingManager_GetFullyOccupiedDates_ReturnsListCount11()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(10);
            DateTime endDate = DateTime.Today.AddDays(20);
            //Act
            List<DateTime> fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);
            //Assert
            Assert.Equal(11, fullyOccupiedDates.Count);
        }


        #endregion

    }
}
