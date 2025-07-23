using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface IKaynakDongusuService
    {
        IEnumerable<KaynakDongusu> GetAll();
        KaynakDongusu? GetById(int id);
        void Add(KaynakDongusu kaynakDongusu);
        void Update(KaynakDongusu kaynakDongusu);
        void Delete(int id);
    }
}
