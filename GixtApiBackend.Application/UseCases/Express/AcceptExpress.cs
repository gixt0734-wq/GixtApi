using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{
    public class AcceptExpress
    {
        private readonly IExpressRepository _repo;
        public AcceptExpress(IExpressRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid express_id,Guid worker_id, decimal km_cost, decimal labor_price)
        {
            await _repo.AcceptExpressAsync(express_id,worker_id,km_cost,labor_price);
        }
    }
}
