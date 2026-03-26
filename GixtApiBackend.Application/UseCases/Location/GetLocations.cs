using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;

namespace GixtApiBackend.Application.UseCases.Locations
{
    public class GetLocation
    {
        private readonly ILocationRepository _repo;
        public GetLocation(ILocationRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<object>> Execute()
        {
            // Ejecutar consulta y devolver resultado
            return await _repo.GetAllLocationsAsync();
        }

    }
}
