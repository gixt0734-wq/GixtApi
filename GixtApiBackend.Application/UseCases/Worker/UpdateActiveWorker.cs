using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Workers
{
    public class UpdateActiveWorker
    {
        private readonly IWorkerRepository _repo;

        public UpdateActiveWorker(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid id )
        {
            await _repo.UpdateActiveWorker(id);
        }
    }
}
