using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;

namespace GixtApiBackend.Application.UseCases.Categories
{
    public class DeleteCategory
    {
        private readonly ICategoryRepository _repository;

        public DeleteCategory(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id)
        {
            await _repository.DeleteCategoryAsync(id);
        }
    }
}
