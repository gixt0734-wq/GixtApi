using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Services
{
    public class GetService
    {
        private readonly IServiceRepository _repo;
        public GetService(IServiceRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<object>> Execute()
        {

            return await _repo.GetAllServicesAsync();
        }

    }
}
