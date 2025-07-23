using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface ITrafoBilgisiService
    {
        IEnumerable<TrafoBilgisi> GetAll();
        TrafoBilgisi? GetById(int id);
        void Add(TrafoBilgisi trafoBilgisi);
        void Update(TrafoBilgisi trafoBilgisi);
        void Delete(int id);
    }
}
