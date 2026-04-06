using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Expresss
{
    public class GetExpressReviewById
    {
        private readonly IExpressRepository _repo;

        public GetExpressReviewById(IExpressRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id, Guid idworker)
        {
            return await _repo.GetExpressReviewIdAsync(id,idworker);
        }
    }
}
