using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Jobs
{
    public class GetJobsByWorkerId
    {
        private readonly IJobRepository _repo;

        public GetJobsByWorkerId(IJobRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid iduser)
        {
            return await _repo.GetJobsByWorkerIdAsync(iduser);
        }
    }
}
