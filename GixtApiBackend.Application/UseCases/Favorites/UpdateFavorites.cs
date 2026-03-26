using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Favorites
{
    public class UpdateFavorite
    {
        private readonly IFavoriteRepository _repo;

        public UpdateFavorite(IFavoriteRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid idServicio, Guid iduser)
        {
            await _repo.UpdateFavoriteAsync(idServicio, iduser);
        }
    }
}
