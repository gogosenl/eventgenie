using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventGenie.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<User>>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return Ok(new Response<IEnumerable<User>> { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<IEnumerable<User>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response<User>>> AddUserAsync(User user)
        {
            try
            {
                await _userRepository.AddUserAsync(user);
                return Ok(new Response<User> { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                return Conflict(new Response<User> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<User>>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new Response<User> { Success = false, Message = $"{id} id'li kullanıcı bulunamadı" });

                return Ok(new Response<User> { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<User> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Response<User>>> UpdateUserAsync(int id, User updatedUser)
        {
            if (!await _userRepository.UserExistsAsync(id))
                return NotFound(new Response<User> { Success = false, Message = "User not found." });

            updatedUser.UserId = id;
            await _userRepository.UpdateUserAsync(updatedUser);
            return Ok(new Response<User> { Success = true, Data = updatedUser, Message = "User updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            if (!await _userRepository.UserExistsAsync(id))
                return NotFound(new Response<User> { Success = false, Message = "User not found." });

            await _userRepository.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("Login")]
        public async Task<ActionResult<Response<object>>> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var email = loginRequest.Email?.Trim();
                var password = loginRequest.Password?.Trim();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest(new Response<object>
                    {
                        Success = false,
                        Message = "Email ve şifre zorunludur."
                    });
                }

                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null || !string.Equals(user.UserEmail?.Trim(), email, StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new Response<object>
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı."
                    });
                }

                // NOT: Şifre plain text karşılaştırılıyor.
                // Prodüksiyon için BCrypt veya SHA256 hash kullanılmalı.
                if (!string.Equals(user.UserPassword?.Trim(), password, StringComparison.Ordinal))
                {
                    return Unauthorized(new Response<object>
                    {
                        Success = false,
                        Message = "Şifre yanlış."
                    });
                }

                return Ok(new Response<object>
                {
                    Success = true,
                    Message = "Giriş başarılı.",
                    Data = new { userId = user.UserId, userName = user.UserName }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpPatch("{id}/preferences")]
        public async Task<IActionResult> UpdateUserPreferencesAsync(int id, [FromBody] string newPreferences)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new Response<User> { Success = false, Message = "User not found." });

                user.Preferences = newPreferences;
                await _userRepository.UpdateUserAsync(user);

                return Ok(new Response<User> { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<User> { Success = false, Message = ex.Message });
            }
        }


        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePasswordAsync(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new Response<object> { Success = false, Message = "Kullanıcı bulunamadı." });

                if (!string.Equals(user.UserPassword?.Trim(), request.CurrentPassword?.Trim(), StringComparison.Ordinal))
                    return BadRequest(new Response<object> { Success = false, Message = "Mevcut şifre yanlış." });

                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                    return BadRequest(new Response<object> { Success = false, Message = "Yeni şifre en az 6 karakter olmalıdır." });

                user.UserPassword = request.NewPassword.Trim();
                await _userRepository.UpdateUserAsync(user);

                return Ok(new Response<object> { Success = true, Message = "Şifre başarıyla değiştirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<object> { Success = false, Message = ex.Message });
            }
        }
    }
}