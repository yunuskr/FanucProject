using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface IAdminService
    {
        IEnumerable<Admin> GetAll();
        Admin? GetById(int id);
        void Add(Admin adminEntity);
        void Update(Admin adminEntity);
        void Delete(int id);
    }
}
