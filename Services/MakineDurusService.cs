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


    }
}
