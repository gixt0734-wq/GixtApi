using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Services
{
    public class UpdateService
    {
        private readonly IServiceRepository _repo;

        public UpdateService(IServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Service servicio)
        {
            await _repo.UpdateServiceAsync(servicio);
        }
    }
}
