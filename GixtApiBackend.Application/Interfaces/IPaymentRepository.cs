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
    public interface IPaymentRepository
    {
        Task UpdatePaymentAsync(PaymentDtos dto);
        Task<IEnumerable<Payment>> GetAllPaymentAsync();
        Task<object> GetPaymentByIdAsync(Guid id);

    }
}
