using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Entities;


namespace GixtApiBackend.Application.Interfaces
{
    public interface ILocationRepository
    {
        Task CreateLocationAsync(LocationDTO dto);
        Task UpdateLocationAsync(LocationDTO dto);
        Task DeleteLocationAsync(Guid id);

        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<object> GetLocationByIdAsync(Guid id);
        Task<object> GetLocationsByUserIdAsync(Guid id);
    }

}
