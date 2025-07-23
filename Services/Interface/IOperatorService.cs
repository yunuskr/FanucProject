using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface IOperatorService
    {
        IEnumerable<Operator> GetAll();
        Operator? GetById(int id);
        void Add(Operator operatorEntity);
        void Update(Operator operatorEntity);
        void Delete(int id);
    }
}