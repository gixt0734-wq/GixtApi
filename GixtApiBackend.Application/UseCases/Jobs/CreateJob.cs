using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Jobs
{
    public class CreateJob
    {
        private readonly IJobRepository _repo;

        public CreateJob(IJobRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(JobDTO dto)
        {
            await _repo.CreateJobAsync(dto);
        }
    }
}
