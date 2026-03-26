using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Users
{
    public class GetUser
    {
        private readonly IUserRepository _repo;
        public GetUser(IUserRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<User>> Execute()
        {
            // Ejecutar consulta y devolver resultado
            return await _repo.GetAllUsersAsync();
        }

    }
}
