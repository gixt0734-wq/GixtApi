using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Categories
{
    public class GetCategory
    {
        private readonly ICategoryRepository _repo;
        public GetCategory(ICategoryRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<object>> Execute()
        {
            return await _repo.GetAllCategoriesAsync();
        }

    }
}
