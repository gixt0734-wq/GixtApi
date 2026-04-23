using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;


namespace GixtApiBackend.Application.Interfaces
{
    public interface IWorkerRepository
    {
        Task CreateWorkerAsync(UserDTO dto);
        Task CreateInfoWorkerAsync(WorkerDTO dto);
        Task UpdateWorkerAsync(UserUpdateDTO dto);
        Task UpdateInfoWorkerAsync(WorkerDTO dto);
        Task DeleteWorkerAsync(Guid id);
        Task<IEnumerable<User>> GetAllWorkersAsync();
        Task<object> GetWorkerByIdAsync(Guid id);
        Task<object> GetInfoWorkerByIdAsync(Guid id);
    }

}
