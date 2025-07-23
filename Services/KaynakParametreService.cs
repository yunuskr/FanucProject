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


    }
}
