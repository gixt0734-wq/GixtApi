using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Favorites
{ 
   public class DeleteFavorite
    {
        private readonly IFavoriteRepository _repository;

        public DeleteFavorite(IFavoriteRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.DeleteFavoriteAsync(id);
        }
    }
}
