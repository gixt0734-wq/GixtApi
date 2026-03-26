using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Services
{
    public class CreateService
    {
        private readonly IServiceRepository _repo;

        public CreateService(IServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(ServiceDTO dto)
        {
            await _repo.CreateServiceAsync(dto);
        }
    }
}
