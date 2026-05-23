using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace GixtApiBackend.Infraestructure
{
    public class ImageService
    {
        private readonly string _basePath;

        public ImageService(IConfiguration config)
        {
            // Lee la ruta de appsettings, con fallback
            _basePath = config["ImageStorage:BasePath"];
        }
        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            // Carpeta fuera de wwwroot
            var directfolder = Path.Combine(_basePath, $"{folder}/");
            if (!Directory.Exists(directfolder))
                Directory.CreateDirectory(directfolder);

            var fileName = $"{Guid.NewGuid()}.webp";
            var fullPath = Path.Combine(directfolder, fileName);

            await SaveOptimizedImageAsync(file, fullPath);

            // El RequestPath que configuramos en Program.cs
            return $"{folder}/{fileName}";
        }


        public async Task DeleteImageAsync( string folder)
        {
            // Carpeta fuera de wwwroot
            var directfolder = Path.Combine(_basePath, $"{folder}/");
            if (!Directory.Exists(directfolder))
                Directory.CreateDirectory(directfolder);

            if (File.Exists(directfolder))
            {
                File.Delete(directfolder);
            }

        }

        public async Task SaveOptimizedImageAsync(IFormFile file, string path, int maxSize = 800, int quality = 10)
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxSize, maxSize),
                Mode = ResizeMode.Max
            }));

            var encoder = new WebpEncoder
            {
                Quality = quality
            };

            await image.SaveAsync(path, encoder);
        }
    }
}