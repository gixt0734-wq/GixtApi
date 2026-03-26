

using GixtApiBackend.Application.UseCases.Users;
using GixtApiBackend.Application.DTos;
using Microsoft.AspNetCore.Mvc;


namespace GixtApiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CreateUser _createUser;
        private readonly GetUser _getUsers;
        private readonly UpdateUser _updateUser;
        private readonly DeleteUser _deleteUser;
        private readonly GetUserById _getUserById;
        private readonly VerficationEmail _verificationemail;

        public UsersController(
            CreateUser createUser,
            GetUser getUsers,
            UpdateUser updateUser,
            DeleteUser deleteUser,
            GetUserById getUserById,
            VerficationEmail verfication

        )
        {
            _createUser = createUser;
            _getUsers = getUsers;
            _updateUser = updateUser;
            _deleteUser = deleteUser;
            _getUserById = getUserById;
            _verificationemail = verfication;
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromForm] UserDTO dto)
        {
            try
            {
                await _createUser.Execute(dto);
                return Ok(new { message = "User added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpPost("verification")]
        public async Task<IActionResult> PostVerification([FromForm] String email)
        {
            try
            {
                var result = await _verificationemail.Execute(email);
                return Ok(new { message = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var users = await _getUsers.Execute();
                return Ok(users);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _getUserById.Execute(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromForm] UserUpdateDTO dto)
        {
            if (id != dto.user_id)
                return BadRequest(new { message = "The user ID does not match the request body." });

            await _updateUser.Execute(dto);
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteUser.Execute(id);
            return Ok(new { message = "User deleted successfully" });
        }
    }

}
