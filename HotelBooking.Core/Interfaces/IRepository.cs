using System.Collections.Generic;
namespace HotelBooking.Core.Interfaces
{
    public interface IRepository<T>
    {
        IList<T> GetAll();
        T Get(int id);
        void Add(T entity);
        void Edit(T entity);
        void Remove(int id);
    }
}
