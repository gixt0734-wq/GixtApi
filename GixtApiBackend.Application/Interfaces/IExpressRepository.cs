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
    public interface IExpressRepository
    {
        Task<Guid> CreateExpressAsync(ExpressDTO dto);
        Task DeleteExpressAsync(Guid id);
        Task CancelExpressAsync(Guid id);
        Task<IEnumerable<Express>> GetAllExpresssAsync();
        Task<object> GetExpressReviewIdAsync(Guid id, Guid idworker);
        Task<object> GetExpressByIdAsync(Guid id);
        //Task<object> GetExpressWorkerByIdAsync(Guid id);
        //Task<object> GetExpresssByUserIdAsync(Guid userId);
        Task<object> GetExpresssByWorkerIdAsync(Guid id);
        Task UpdateExpressStatusAsync(Guid id, String action);
        Task AcceptExpressAsync(Guid express_id ,Guid worker_id,decimal km_cost, decimal labor_price);
        Task SendAcceptAsync(Guid worker, Guid id, decimal km_cost, decimal labor_price);

    }
}
