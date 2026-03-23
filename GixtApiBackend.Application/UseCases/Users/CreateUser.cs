using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;

namespace GixtApiBackend.Application.UseCases.Users
{
    public class CreateUser
    {
        private readonly IUserRepository _repo;

        public CreateUser(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(UserDTO dto)
        {
            await _repo.CreateUserAsync(dto);
        }
    }
}
