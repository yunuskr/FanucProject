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

        public IEnumerable<Operator> GetAll()
        {
            return _context.Operators;
        }

        public Operator? GetById(int id)
        {
            return _context.Operators.Find(id);
        }

        public void Add(Operator operatorEntity)
        {
            _context.Operators.Add(operatorEntity);
            _context.SaveChanges();
        }

        public void Update(Operator operatorEntity)
        {
            _context.Operators.Update(operatorEntity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Operators.Find(id);
            if (entity != null)
            {
                _context.Operators.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}