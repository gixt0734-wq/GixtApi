using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Workers
{
    public class CreateInfoWorker
    {
        private readonly IWorkerRepository _repo;

        public CreateInfoWorker(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(WorkerDTO dto)
        {
            await _repo.CreateInfoWorkerAsync(dto);
        }
    }
}
