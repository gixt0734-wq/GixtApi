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
    public class GetJobsByUserId
    {
        private readonly IJobRepository _repo;

        public GetJobsByUserId(IJobRepository repo)
        {
            _repo = repo;
        }

        public async Task<Object?> Execute(Guid iduser)
        {
            return await _repo.GetJobsByUserIdAsync(iduser);
        }
    }
}
