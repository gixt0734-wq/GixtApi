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
    public class GetInfoWorkerById
    {
        private readonly IWorkerRepository _repo;

        public GetInfoWorkerById(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id)
        {
            return await _repo.GetInfoWorkerByIdAsync(id);
        }
    }
}
