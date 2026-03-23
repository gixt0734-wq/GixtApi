using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Advertisements
{
    public class UpdateAdvertisement
    {
        private readonly IAdvertisementRepository _repository;

        public UpdateAdvertisement(IAdvertisementRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Advertisement dto)
        {
            await _repository.UpdateAdvertisementAsync(dto);
        }
    }

}
