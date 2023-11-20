using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HotelBooking.Core.BindingModels;
using HotelBooking.Core.Entities;
using HotelBooking.Core.Exceptions;
using HotelBooking.Core.Interfaces;
namespace HotelBooking.Core.Services
{
    public class BookingManager : IBookingManager
    {
        private IRepository<Customer> customerRepository;
        private IRepository<Booking> bookingRepository;
        private IRepository<Room> roomRepository;

        // Constructor injection
        public BookingManager(IRepository<Booking> bookingRepository, IRepository<Room> roomRepository, IRepository<Customer> customerRepository)
        {
            this.customerRepository = customerRepository;
            this.bookingRepository = bookingRepository;
            this.roomRepository = roomRepository;
        }

        public bool CreateBooking(BookingPostBindingModel model) {
            var customer = customerRepository.Get(model.CustomerId);
            if (customer is null) {
                throw new RestException(HttpStatusCode.NotFound, "Customer not found");
            }
            int roomId = FindAvailableRoom(model.StartDate, model.EndDate);

            if (roomId >= 0) {
                var booking = new Booking(model) {
                    RoomId = roomId,
                    IsActive = true
                };
                bookingRepository.Add(booking);
                return true;
            }
            return false;
        }

        public int FindAvailableRoom(DateTime startDate, DateTime endDate)
        {
            if (startDate <= DateTime.Today || startDate > endDate)
                throw new RestException(HttpStatusCode.BadRequest,"The start date cannot be in the past or later than the end date.");

            var activeBookings = bookingRepository.GetAll().Where(b => b.IsActive).ToList();
            foreach (var room in roomRepository.GetAll())
            {
                var activeBookingsForCurrentRoom = activeBookings.Where(b => b.RoomId == room.Id);
                if (activeBookingsForCurrentRoom.All(b => startDate < b.StartDate &&
                    endDate < b.StartDate || startDate > b.EndDate && endDate > b.EndDate))
                {
                    return room.Id;
                }
            }
            return -1;
        }

        public List<DateTime> GetFullyOccupiedDates(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new RestException(HttpStatusCode.BadRequest,"The start date cannot be later than the end date.");

            List<DateTime> fullyOccupiedDates = new();
            int noOfRooms = roomRepository.GetAll().Count();
            List<Booking> bookings = bookingRepository.GetAll().ToList();

            if (bookings.Any())
            {
                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    var noOfBookings = bookings.Where(b => b.IsActive && d >= b.StartDate && d <= b.EndDate);
                    if (noOfBookings.Count() >= noOfRooms) 
                        fullyOccupiedDates.Add(d);
                }
            }
            return fullyOccupiedDates;
        }
    }
}
