using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace GixtApiBackend.Infraestructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _basePath;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CategoryRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _context = context;
            _basePath = config["ImageStorage:BasePath"];
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateCategoryAsync(CategoryDTO dto)
        {
            string? rutaCompleta = null;
            try
            {
                var category = new Category
                {
                    name = dto.name
                };
                category.is_active = true;
                if (dto.image != null && dto.image.Length > 0)
                {
                    var folder = Path.Combine(_basePath, "category");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
                    rutaCompleta = Path.Combine(folder, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await dto.image.CopyToAsync(stream);
                    }

                    // Guardar la ruta accesible desde la web
                   category.image_url = "/img/category/" + nombreArchivo;
                }
                await _context.categories.AddAsync(category);
                await _context.SaveChangesAsync();

            }
            catch
            {
                if (!string.IsNullOrEmpty(rutaCompleta) && File.Exists(rutaCompleta))
                {
                    File.Delete(rutaCompleta);
                }

                throw; // relanza error


            }
    
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.categories.FindAsync(id);
            if (category != null)
            {
                _context.categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            var existing = await _context.categories.FindAsync(category.category_id);
            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Object>> GetAllCategoriesAsync()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
               from s in _context.categories
               where s.is_active == true
               select new
               {
                   s.name,
                   s.category_id,
                   Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
               }
               ).ToListAsync();
            return result;
        }
                

    }

}
