using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Jobs
{ 
   public class CancelJob
    {
        private readonly IJobRepository _repository;

        public CancelJob(IJobRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.CancelJobAsync(id);
        }
    }
}
