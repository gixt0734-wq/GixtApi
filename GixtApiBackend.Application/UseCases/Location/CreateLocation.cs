using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Locations
{
    public class CreateLocation
    {
        private readonly ILocationRepository _repo;

        public CreateLocation(ILocationRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(LocationDTO dto)
        {
            await _repo.CreateLocationAsync(dto);
        }
    }
}
