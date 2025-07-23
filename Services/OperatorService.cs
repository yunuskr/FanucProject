using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;

namespace FanucRelease.Services
{
    public class OperatorService : IOperatorService
    {
        private readonly ApplicationDbContext _context;

        public OperatorService(ApplicationDbContext context)
        {
            _context = context;
        }

      
    }
}