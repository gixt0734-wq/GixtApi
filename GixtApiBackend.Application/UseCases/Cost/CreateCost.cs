using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Cost
{
    public class CreateCost
    {
        private readonly ICostRepository _repo;

        public CreateCost(ICostRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(CostsDtos dto)
        {
            await _repo.CreateCostAsync(dto);
        }
    }
}
