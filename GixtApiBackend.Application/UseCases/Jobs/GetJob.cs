using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Jobs
{
    public class GetJob
    {
        private readonly IJobRepository _repo;
        public GetJob(IJobRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Job>> Execute()
        {

            return await _repo.GetAllJobsAsync();
        }

    }
}
