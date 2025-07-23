using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;

namespace FanucRelease.Services
{
    public class KaynakParametreService : IKaynakParametreService
    {
        private readonly ApplicationDbContext _context;

        public KaynakParametreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<KaynakParametre> GetAll()
        {
            return _context.KaynakParametreleri;
        }

        public KaynakParametre? GetById(int id)
        {
            return _context.KaynakParametreleri.Find(id);
        }

        public void Add(KaynakParametre kaynakParametre)
        {
            _context.KaynakParametreleri.Add(kaynakParametre);
            _context.SaveChanges();
        }

        public void Update(KaynakParametre kaynakParametre)
        {
            _context.KaynakParametreleri.Update(kaynakParametre);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.KaynakParametreleri.Find(id);
            if (entity != null)
            {
                _context.KaynakParametreleri.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
