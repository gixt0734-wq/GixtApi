using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Favorites
{
    public class CreateFavorite
    {
        private readonly IFavoriteRepository _repo;

        public CreateFavorite(IFavoriteRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid idServicio, Guid iduser)
        {
            await _repo.CreateFavoriteAsync(idServicio, iduser);
        }
    }
}
