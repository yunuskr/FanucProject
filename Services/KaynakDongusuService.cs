using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;

namespace FanucRelease.Services
{
    public class KaynakDongusuService : IKaynakDongusuService
    {
        private readonly ApplicationDbContext _context;

        public KaynakDongusuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<KaynakDongusu> GetAll()
        {
            return _context.KaynakDonguleri;
        }

        public KaynakDongusu? GetById(int id)
        {
            return _context.KaynakDonguleri.Find(id);
        }

        public void Add(KaynakDongusu kaynakDongusu)
        {
            _context.KaynakDonguleri.Add(kaynakDongusu);
            _context.SaveChanges();
        }

        public void Update(KaynakDongusu kaynakDongusu)
        {
            _context.KaynakDonguleri.Update(kaynakDongusu);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.KaynakDonguleri.Find(id);
            if (entity != null)
            {
                _context.KaynakDonguleri.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
