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
    public class UpdateWorker
    {
        private readonly IWorkerRepository _repo;

        public UpdateWorker(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(UserUpdateDTO dto)
        {
            await _repo.UpdateWorkerAsync(dto);
        }
    }
}
