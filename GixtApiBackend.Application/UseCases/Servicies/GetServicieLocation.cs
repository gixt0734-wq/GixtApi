using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Services
{
    public class GetServiceLocation
    {
        private readonly IServiceRepository _repo;
        public GetServiceLocation(IServiceRepository repo)
        {
            _repo = repo;
        }
        public async Task<object?> Execute(decimal latitude,decimal longitude,double rangoKm)
        {
            return await _repo.GetAllServicesLocationAsync(latitude, longitude,rangoKm);
        }

    }
}
