using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;

namespace FanucRelease.Services
{
    public class TrafoBilgisiService : ITrafoBilgisiService
    {
        private readonly ApplicationDbContext _context;

        public TrafoBilgisiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<TrafoBilgisi> GetAll()
        {
            return _context.TrafoBilgileri;
        }

        public TrafoBilgisi? GetById(int id)
        {
            return _context.TrafoBilgileri.Find(id);
        }

        public void Add(TrafoBilgisi trafoBilgisi)
        {
            _context.TrafoBilgileri.Add(trafoBilgisi);
            _context.SaveChanges();
        }

        public void Update(TrafoBilgisi trafoBilgisi)
        {
            _context.TrafoBilgileri.Update(trafoBilgisi);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.TrafoBilgileri.Find(id);
            if (entity != null)
            {
                _context.TrafoBilgileri.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
