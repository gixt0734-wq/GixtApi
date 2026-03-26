
using GixtApiBackend.Application.UseCases.Categories;
using GixtApiBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GixtApiBackend.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CreateCategory _createCategory;
        private readonly GetCategory _getCategories;
        private readonly UpdateCategory _updateCategory;
        private readonly DeleteCategory _deleteCategory;

        public CategoriesController(
            CreateCategory createCategory,
            GetCategory getCategories,
            UpdateCategory updateCategory,
            DeleteCategory deleteCategory
        )
        {
            _createCategory = createCategory;
            _getCategories = getCategories;
            _updateCategory = updateCategory;
            _deleteCategory = deleteCategory;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CategoryDTO category)
        {
            await _createCategory.Execute(category);
            return Ok(new { message = "Category added successfully" });
        }

        [HttpGet]
        
        public async Task<IActionResult> Get()
        {
            var categories = await _getCategories.Execute();
            return Ok(categories);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Category category)
        {
            if (id != category.category_id)
                return BadRequest(new { message = "The ID does not match the request body." });

            await _updateCategory.Execute(category);
            return Ok(new { message = "Category updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _deleteCategory.Execute(id);
            return Ok(new { message = "Category deleted successfully" });
        }
    }

}
