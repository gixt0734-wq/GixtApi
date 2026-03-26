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
    public class GetServiceById
    {
        private readonly IServiceRepository _repo;

        public GetServiceById(IServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id, Guid iduser)
        {
            return await _repo.GetServiceByIdAsync(id, iduser);
        }
    }
}
