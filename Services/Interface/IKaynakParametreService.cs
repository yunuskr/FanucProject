using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface IKaynakParametreService
    {
        IEnumerable<KaynakParametre> GetAll();
        KaynakParametre? GetById(int id);
        void Add(KaynakParametre kaynakParametre);
        void Update(KaynakParametre kaynakParametre);
        void Delete(int id);
    }
}
