using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Expresss
{
    public class CreateExpress
    {
        private readonly IExpressRepository _repo;

        public CreateExpress(IExpressRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> Execute(ExpressDTO dto)
        {
           return await _repo.CreateExpressAsync(dto);
        }
    }
}
