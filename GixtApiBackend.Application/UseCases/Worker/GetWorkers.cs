using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Workers
{
    public class GetWorkers
    {
        private readonly IWorkerRepository _repo;
        public GetWorkers(IWorkerRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<User>> Execute()
        {
            // Ejecutar consulta y devolver resultado
            return await _repo.GetAllWorkersAsync();
        }

    }
}
