using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.Core.BindingModels;
using HotelBooking.Core.Entities;
using HotelBooking.Core.Exceptions;
using HotelBooking.Core.Interfaces;
using HotelBooking.Core.Services;
using HotelBooking.UnitTests.Fakes;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;

        public BookingManagerTests()
        {
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            IRepository<Booking> bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            IRepository<Customer> customerRepository = new FakeCustomerRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository,customerRepository);
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
        public void BookingManager_GetFullyOccupiedDates_ReturnsListCount10()
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
