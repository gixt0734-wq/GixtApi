using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{
    public class GetExpress
    {
        private readonly IExpressRepository _repo;
        public GetExpress(IExpressRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Express>> Execute()
        {

            return await _repo.GetAllExpresssAsync();
        }

    }
}
