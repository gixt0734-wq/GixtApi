using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Categories
{
    public class CreateCategory
    {
        private readonly ICategoryRepository _repo;

        public CreateCategory(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(CategoryDTO categoria)
        {
            await _repo.CreateCategoryAsync(categoria);
        }
    }
}
