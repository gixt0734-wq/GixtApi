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
    public class UpdateInfoWorker
    {
        private readonly IWorkerRepository _repo;

        public UpdateInfoWorker(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(WorkerDTO dto)
        {
            await _repo.UpdateInfoWorkerAsync(dto);
        }
    }
}
