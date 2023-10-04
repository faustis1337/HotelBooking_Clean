using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IBookingManager bookingManager;
        private readonly Mock<IRepository<Room>> fakeRoomRepository;
        private readonly Mock<IRepository<Booking>> fakeBookingRepository;
        private readonly Mock<IRepository<Customer>> fakeCustomerRepository;

        public BookingManagerTests()
        {
            fakeBookingRepository = new ();
            fakeRoomRepository = new ();
            fakeCustomerRepository = new ();
            bookingManager = new BookingManager(fakeBookingRepository.Object, fakeRoomRepository.Object, fakeCustomerRepository.Object);
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
            DateTime date = DateTime.Today.AddDays(30);
            
            // TODO: add the room adding logic
            fakeRoomRepository.Setup(x => x.GetAll()).Returns(new List<Room> {
                new(){ Id = 1},
                new(){ Id = 2}
            });
            
            fakeBookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking> {
                new() {IsActive = true, RoomId = 1, StartDate = DateTime.Today.AddDays(36), EndDate = DateTime.Today.AddDays(40)},
                new() {IsActive = true, RoomId = 2, StartDate = DateTime.Today.AddDays(30), EndDate = DateTime.Today.AddDays(32)},
                new() {IsActive = true, RoomId = 1, StartDate = DateTime.Today.AddDays(22), EndDate = DateTime.Today.AddDays(29)},
            });

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
            
            // TODO: add the room adding logic
            fakeRoomRepository.Setup(x => x.GetAll()).Returns(new List<Room> {
                new(){ Id = 1},
                new(){ Id = 2}
            });
            
            fakeBookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking> {
                new() {IsActive = true, RoomId = 1, StartDate = DateTime.Today.AddDays(36), EndDate = DateTime.Today.AddDays(40)},
                new() {IsActive = true, RoomId = 2, StartDate = DateTime.Today.AddDays(30), EndDate = DateTime.Today.AddDays(32)},
                new() {IsActive = true, RoomId = 1, StartDate = DateTime.Today.AddDays(22), EndDate = DateTime.Today.AddDays(29)},
            });
            
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
            fakeCustomerRepository.Setup(x => x.Get(1)).Returns(new Customer {
                Id = 1
            });
            fakeRoomRepository.Setup(x => x.GetAll()).Returns(new List<Room> {
                new(),
                new(),
                new()
            });
            fakeBookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking>());
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
        public void BookingManager_GetFullyOccupiedDates_EndBeforeStart_ThrowRestException()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(5);
            DateTime endDate = DateTime.Today.AddDays(2);

            // Act and Assert
            var exception = Assert.Throws<RestException>(() =>
            {
                bookingManager.GetFullyOccupiedDates(startDate, endDate);
            });
            
            Assert.Equal(HttpStatusCode.BadRequest,exception.Status);
        }

        [Fact]
        public void BookingManager_GetFullyOccupiedDates_ReturnsEmptyList()
        {
            //Arrange
            fakeRoomRepository.Setup(x => x.GetAll()).Returns(new List<Room> {
                new(),
                new(),
                new()
            });
            fakeBookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking> {
                new() {IsActive = true, StartDate = DateTime.Today.AddDays(24), EndDate = DateTime.Today.AddDays(30)},
                new() {IsActive = true, StartDate = DateTime.Today.AddDays(5), EndDate = DateTime.Today.AddDays(23)},
                new() {IsActive = true, StartDate = DateTime.Today.AddDays(20), EndDate = DateTime.Today.AddDays(30)},
                new() {IsActive = false, StartDate = DateTime.Today.AddDays(20), EndDate = DateTime.Today.AddDays(30)},
            });
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
            fakeRoomRepository.Setup(x => x.GetAll()).Returns(new List<Room> {
                new(),
                new(),
                new()
            });
            fakeBookingRepository.Setup(x => x.GetAll()).Returns(new List<Booking> {
                new() {IsActive = true, StartDate = DateTime.Today.AddDays(10), EndDate = DateTime.Today.AddDays(21)},
                new() {IsActive = true, StartDate = DateTime.Today.AddDays(2), EndDate = DateTime.Today.AddDays(20)},
                new() {IsActive = true, StartDate = DateTime.Today.AddDays(8), EndDate = DateTime.Today.AddDays(30)},
            });
            //Act
            List<DateTime> fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);
            //Assert
            Assert.Equal(11, fullyOccupiedDates.Count);
        }


        #endregion

    }
}
