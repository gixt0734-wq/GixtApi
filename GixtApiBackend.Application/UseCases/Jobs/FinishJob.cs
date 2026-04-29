using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Jobs
{
    public class FinishJob
    {
        private readonly IJobRepository _repository;

        public FinishJob(IJobRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.FinishJobAsync(id);
        }
    }
}
