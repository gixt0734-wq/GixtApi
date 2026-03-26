using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Advertisements
{
    public class GetAdvertisement
    {
        private readonly IAdvertisementRepository _repo;
        public GetAdvertisement(IAdvertisementRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Advertisement>> Execute()
        {
            return await _repo.GetAllAdvertisementsAsync();
        }

    }
}
