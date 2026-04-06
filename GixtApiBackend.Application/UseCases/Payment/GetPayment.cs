using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Paymentss
{
    public class GetPayment
    {
        private readonly IPaymentRepository _repo;
        public GetPayment(IPaymentRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Payment>> Execute()
        {

            return await _repo.GetAllPaymentAsync();
        }

    }
}
