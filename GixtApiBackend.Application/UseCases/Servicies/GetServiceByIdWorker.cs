using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Services
{
    public class GetServiceByIdWorker
    {
        private readonly IServiceRepository _repo;

        public GetServiceByIdWorker(IServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id)
        {
            return await _repo.GetServiceByIdWorkerAsync(id);
        }
    }
}
