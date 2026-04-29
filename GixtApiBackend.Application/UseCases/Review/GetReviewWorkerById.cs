using GixtApiBackend.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Application.UseCases.Review
{
    public class GetReviewWorkerById
    {
        private readonly IReviewRepository _repo;

        public GetReviewWorkerById(IReviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid id)
        {
            return await _repo.GetReviewWorkerByIdAsync(id);
        }
    }
}
