using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Domain.Entities;


namespace GixtApiBackend.Application.Interfaces
{
    public interface IAdvertisementRepository
    {
        Task CreateAdvertisementAsync(AdvertisementDTO advertisement);
        Task UpdateAdvertisementAsync(Advertisement advertisement);
        Task DeleteAdvertisementAsync(int id);
        Task<IEnumerable<Advertisement>> GetAllAdvertisementsAsync();
    }

}
