using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AdminController(
            IUserService userService,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(); 

            return Ok(user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest model)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(userId, model);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }

        [HttpPost("users/{id}/roles")]
        public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleRequest model)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var result = await _userService.AssignRoleToUserAsync(userId, model.RoleName);
            if (!result)
                return BadRequest("نقش با موفقیت به کاربر اختصاص داده نشد");

            return Ok(new { message = "نقش با موفقیت به کاربر اختصاص داده شد" });
        }

        [HttpDelete("users/{id}/roles")]
        public async Task<IActionResult> RemoveRole(string id, [FromBody] RemoveRoleRequest model)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var result = await _userService.RemoveRoleFromUserAsync(userId, model.RoleName);
            if (!result)
                return BadRequest("نقش با موفقیت از کاربر حذف نشد");

            return Ok(new { message = "نقش با موفقیت از کاربر حذف شد" });
        }
    }
}