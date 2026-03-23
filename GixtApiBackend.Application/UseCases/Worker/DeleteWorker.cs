using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Workers
{ 
   public class DeleteWorker
    {
        private readonly IWorkerRepository _repository;

        public DeleteWorker(IWorkerRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.DeleteWorkerAsync(id);
        }
    }
}
