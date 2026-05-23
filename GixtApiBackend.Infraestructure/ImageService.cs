using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace GixtApiBackend.Infraestructure
{
    public class ImageService
    {
        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            // Carpeta fuera de wwwroot
            var directfolder = Path.Combine(Directory.GetCurrentDirectory(), $"PrivateImages/img/{folder}/");
            if (!Directory.Exists(directfolder))
                Directory.CreateDirectory(directfolder);

            var fileName = $"{Guid.NewGuid()}.webp";
            var fullPath = Path.Combine(directfolder, fileName);

            await SaveOptimizedImageAsync(file, fullPath);

            // El RequestPath que configuramos en Program.cs
            return $"/private-images/img/{folder}/{fileName}";
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