using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;

namespace FanucRelease.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Admin> GetAll()
        {
            return _context.Admins;
        }

        public Admin? GetById(int id)
        {
            return _context.Admins.Find(id);
        }

        public void Add(Admin adminEntity)
        {
            _context.Admins.Add(adminEntity);
            _context.SaveChanges();
        }

        public void Update(Admin adminEntity)
        {
            _context.Admins.Update(adminEntity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Admins.Find(id);
            if (entity != null)
            {
                _context.Admins.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
