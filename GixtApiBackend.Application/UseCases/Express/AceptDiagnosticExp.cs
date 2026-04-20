using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{ 
   public class FinishExpress
    {
        private readonly IExpressRepository _repository;

        public FinishExpress(IExpressRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.FinishExpressAsync(id);
        }
    }
}
