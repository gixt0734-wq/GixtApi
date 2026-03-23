using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Domain.Entities;


namespace GixtApiBackend.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task CreateCategoryAsync(CategoryDTO category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task<IEnumerable<object>> GetAllCategoriesAsync();
    }

}
