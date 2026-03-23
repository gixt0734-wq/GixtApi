using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Favorites
{
    public class GetFavorite
    {
        private readonly IFavoriteRepository _repo;
        public GetFavorite(IFavoriteRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<object>> Execute()
        {

            return await _repo.GetAllFavoritesAsync();
        }

    }
}
