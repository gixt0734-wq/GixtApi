using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Locations
{
    public class UpdateLocation
    {
        private readonly ILocationRepository _repo;

        public UpdateLocation(ILocationRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(LocationDTO dto)
        {
            await _repo.UpdateLocationAsync(dto);
        }
    }
}
