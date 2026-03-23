using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Advertisements
{
    public class CreateAdvertisement
    {
        private readonly IAdvertisementRepository _repo;

        public CreateAdvertisement(IAdvertisementRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(AdvertisementDTO dto)
        {
            await _repo.CreateAdvertisementAsync(dto);
        }
    }
}
