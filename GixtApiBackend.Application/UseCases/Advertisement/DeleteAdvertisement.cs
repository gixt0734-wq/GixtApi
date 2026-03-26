using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;

namespace GixtApiBackend.Application.UseCases.Advertisements
{
    public class DeleteAdvertisement
    {
        private readonly IAdvertisementRepository _repository;

        public DeleteAdvertisement(IAdvertisementRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id)
        {
            await _repository.DeleteAdvertisementAsync(id);
        }
    }
}
