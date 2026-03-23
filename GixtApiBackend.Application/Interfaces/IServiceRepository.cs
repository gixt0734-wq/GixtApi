using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.UseCases.Users;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.Interfaces
{
    public interface IServiceRepository
    {
        Task CreateServiceAsync(ServiceDTO dto);
        Task UpdateServiceAsync(Service service);
        Task DeleteServiceAsync(Guid id);

        Task<IEnumerable<object>> GetAllServicesAsync();
        Task<object> GetServiceByIdAsync(Guid id, Guid userId);
        Task<object>  GetServiceByIdWorkerAsync (Guid id);
        Task<object> GetServicesByCategoryAsync(int id, int pageNumber = 1);
    }

}
