using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{
    public class SendAccept
    {
        private readonly IExpressRepository _repo;

        public SendAccept(IExpressRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid worker, Guid id, decimal km_cost, decimal labor_price)
        {
            await _repo.SendAcceptAsync(worker, id, km_cost,labor_price);
    }
}
}
