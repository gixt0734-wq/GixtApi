using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Paymentss
{
    public class UpdatePayment
    {
        private readonly IPaymentRepository _repo;

        public UpdatePayment(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(PaymentDtos dto)
        {
            await _repo.UpdatePaymentAsync(dto);
        }
    }
}
