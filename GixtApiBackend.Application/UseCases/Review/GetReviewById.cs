using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Application.UseCases.Review
{
    public class GetReviewById
    {
        private readonly IReviewRepository _repo;

        public GetReviewById(IReviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id)
        {
            return await _repo.GetReviewByIdAsync(id);
        }
    }
}
