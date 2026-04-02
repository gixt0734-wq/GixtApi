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
    public interface IEvidenceRepository
    {
        Task CreateEvidenceAsync(EvidenceDTO dto);
        //Task<IEnumerable<Costs>> GetAllCostAsync();
        //Task<object> GetCostByIdAsync(Guid id);

    }
}
