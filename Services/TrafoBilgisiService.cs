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


    }
}
