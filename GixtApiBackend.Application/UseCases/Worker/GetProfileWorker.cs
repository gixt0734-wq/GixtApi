using GixtApiBackend.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Application.UseCases.Workers
{
    public class GetProfileWorker
    {
        private readonly IWorkerRepository _repo;

        public GetProfileWorker(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id)
        {
            return await _repo.GetProfileWorker(id);
        }
    }
}
