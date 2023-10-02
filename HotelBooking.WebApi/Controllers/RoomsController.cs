using System.ComponentModel.DataAnnotations;
using System.Net;
using HotelBooking.Core;
using HotelBooking.Core.Exceptions;
using HotelBooking.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomsController : Controller
    {
        private readonly IRepository<Room> repository;

        public RoomsController(IRepository<Room> repos)
        {
            repository = repos;
        }

        // GET: rooms
        [HttpGet(Name = "GetRooms")]
        public IEnumerable<Room> Get()
        {
            return repository.GetAll();
        }

        // GET rooms/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null) {
                throw new RestException(HttpStatusCode.NotFound, "Room not found");
            }
            return new ObjectResult(item);
        }

        // POST rooms
        [HttpPost]
        public IActionResult Post([FromBody] [Required] Room room)
        {
            repository.Add(room);
            return CreatedAtRoute("GetRooms", null);
        }


        // DELETE rooms/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var room = repository.Get(id);
            if (room == null) {
                throw new RestException(HttpStatusCode.NotFound, "Room not found");
            }
            repository.Remove(id);
            return NoContent();
        }

    }
}
