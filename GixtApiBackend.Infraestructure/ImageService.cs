using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace GixtApiBackend.Infraestructure
{
    public class ImageService
    {
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
