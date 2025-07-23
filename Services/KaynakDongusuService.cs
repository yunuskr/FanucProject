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
    }
}
