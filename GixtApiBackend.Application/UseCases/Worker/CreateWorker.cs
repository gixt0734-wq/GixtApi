using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Workers
{
    public class CreateWorker
    {
        private readonly IWorkerRepository _repo;

        public CreateWorker(IWorkerRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(UserDTO dto)
        {
            await _repo.CreateWorkerAsync(dto);
        }
    }
}
