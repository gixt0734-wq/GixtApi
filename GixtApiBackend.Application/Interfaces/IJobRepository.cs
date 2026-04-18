using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;


namespace GixtApiBackend.Application.Interfaces
{
    public interface IJobRepository
    {
        Task CreateJobAsync(JobDTO dto);
        Task DeleteJobAsync(Guid id);
        Task CancelJobAsync(Guid id);
        Task<IEnumerable<Job>> GetAllJobsAsync();
        Task<object> GetReviewJobByIdAsync(Guid id);
        Task<object> GetReviewJobByIdWorkerAsync(Guid id);
        Task<object> GetJobsByUserIdAsync(Guid userId);
        Task<object> GetJobsByWorkerIdAsync(Guid id);
        Task UpdateJobStatusAsync(Guid id, String action);

    }
}
