using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.UseCases.Users;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;


namespace GixtApiBackend.Application.Interfaces
{
    public interface IFavoriteRepository
    {
        Task CreateFavoriteAsync(Guid serviceId, Guid userId);
        Task UpdateFavoriteAsync(Guid serviceId, Guid userId);
        Task DeleteFavoriteAsync(Guid id);
        Task<IEnumerable<Favorite>> GetAllFavoritesAsync();
        Task<object> GetFavoriteByIdAsync(Guid id);
    }

}
