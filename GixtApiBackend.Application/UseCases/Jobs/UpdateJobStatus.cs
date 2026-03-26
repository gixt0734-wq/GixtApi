using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Jobs
{
    public class UpdateJobStatus
    {
        private readonly IJobRepository _repo;

        public UpdateJobStatus(IJobRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(Guid id, String action)
        {
            await _repo.UpdateJobStatusAsync(id,action);
        }
    }
}
