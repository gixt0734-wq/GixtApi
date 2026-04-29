using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Application.UseCases.Review
{
    public class CreateReview
    {
        private readonly IReviewRepository _repo;

        public CreateReview(IReviewRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(ReviewDTO dto)
        {
            await _repo.CreateReviewAsync(dto);
        }
    }
}
