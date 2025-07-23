using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface IMakineDurusService
    {
        IEnumerable<MakineDurus> GetAll();
        MakineDurus? GetById(int id);
        void Add(MakineDurus makineDurus);
        void Update(MakineDurus makineDurus);
        void Delete(int id);
    }
}
