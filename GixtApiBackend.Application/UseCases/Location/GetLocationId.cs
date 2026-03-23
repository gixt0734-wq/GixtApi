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
    public class GetLocationById
    {
        private readonly ILocationRepository _repo;

        public GetLocationById(ILocationRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id)
        {
            return await _repo.GetLocationByIdAsync(id);
        }
    }
}
