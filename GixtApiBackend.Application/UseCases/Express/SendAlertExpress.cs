using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{ 
   public class SendAlertExpress
    {
        private readonly IExpressRepository _repository;

        public SendAlertExpress(IExpressRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.SendAlertExpress(id);
        }
    }
}
