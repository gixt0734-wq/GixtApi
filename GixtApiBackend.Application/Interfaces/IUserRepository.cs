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
    public interface IUserRepository
    {
        Task CreateUserAsync(UserDTO dto);
        Task UpdateUserAsync(UserUpdateDTO dto);
        Task DeleteUserAsync(Guid id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<object> GetUserByIdAsync(Guid id);
        Task<String> VerficationEmailAsync(string email);
    }

}
