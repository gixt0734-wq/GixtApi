using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Users
{
    public class VerficationEmail
    {
        private readonly IUserRepository _repo;

        public VerficationEmail(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> Execute(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El correo es obligatorio.");
            }

            var codigo = await _repo.VerficationEmailAsync(email);

            return codigo;
        }
    }
}
