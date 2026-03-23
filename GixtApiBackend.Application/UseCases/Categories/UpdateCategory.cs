using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Categories
{
    public class UpdateCategory
    {
        private readonly ICategoryRepository _repo;

        public UpdateCategory(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Category categoria)
        {
            await _repo.UpdateCategoryAsync(categoria);
        }
    }
}
