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
    public class GetServiceByCategory
    {
        private readonly IServiceRepository _repo;

        public GetServiceByCategory(IServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(int id, decimal latitude, decimal longitude, double rangoKm, int pageNumber = 1)
        {
            return await _repo.GetServicesByCategoryAsync(id ,latitude,longitude,rangoKm, pageNumber);
        }
    }
}
