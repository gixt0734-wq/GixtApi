using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{
    public class UpdateExpressStatus
    {
        private readonly IExpressRepository _repo;

        public UpdateExpressStatus(IExpressRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid id, String action)
        {
            await _repo.UpdateExpressStatusAsync(id,action);
        }
    }
}
