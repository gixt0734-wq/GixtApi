using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Evidence
{
    public class CreateEvidence
    {
        private readonly IEvidenceRepository _repo;

        public CreateEvidence(IEvidenceRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(EvidenceDTO dto)
        {
            await _repo.CreateEvidenceAsync(dto);
        }
    }
}
