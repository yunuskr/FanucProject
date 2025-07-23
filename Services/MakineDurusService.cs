using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;

namespace FanucRelease.Services
{
    public class MakineDurusService : IMakineDurusService
    {
        private readonly ApplicationDbContext _context;

        public MakineDurusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<MakineDurus> GetAll()
        {
            return _context.MakineDuruslari;
        }

        public MakineDurus? GetById(int id)
        {
            return _context.MakineDuruslari.Find(id);
        }

        public void Add(MakineDurus makineDurus)
        {
            _context.MakineDuruslari.Add(makineDurus);
            _context.SaveChanges();
        }

        public void Update(MakineDurus makineDurus)
        {
            _context.MakineDuruslari.Update(makineDurus);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.MakineDuruslari.Find(id);
            if (entity != null)
            {
                _context.MakineDuruslari.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
