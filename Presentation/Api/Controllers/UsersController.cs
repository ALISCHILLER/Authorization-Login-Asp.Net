using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Presentation.Api.Controllers;
using FeaturesUsersQueries = Authorization_Login_Asp.Net.Core.Application.Features.Users.Queries;
using FeaturesUsersCommands = Authorization_Login_Asp.Net.Core.Application.Features.Users.Commands;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : BaseApiController
    {
        public UsersController(IMediator mediator, ILogger<UsersController> logger) : base(logger, mediator) { }

        /// <summary>
        /// دریافت اطلاعات کاربر با شناسه
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");
            return await ExecuteCommand<UserDto>(new FeaturesUsersQueries.GetUserByIdQuery { UserId = userId });
        }

        /// <summary>
        /// دریافت لیست تمام کاربران
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(PaginatedList<UserDto>), 200)]
        public async Task<IActionResult> GetAllUsers([FromQuery] FeaturesUsersQueries.GetUsersQuery query)
        {
            return await ExecuteCommand<PaginatedList<UserDto>>(query);
        }

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser([FromBody] FeaturesUsersCommands.CreateUserCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;
            var result = await ExecuteCommand<UserDto>(command, "خطا در ایجاد کاربر");
            if (result is OkObjectResult okResult && okResult.Value is UserDto userResponse)
                return CreatedAtAction(nameof(GetUserById), new { id = userResponse.Id }, userResponse);
            return result;
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] FeaturesUsersCommands.UpdateUserCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");
            command.UserId = userId;
            return await ExecuteCommand<UserDto>(command, "خطا در به‌روزرسانی کاربر");
        }

        /// <summary>
        /// حذف کاربر
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");
            await ExecuteCommand<bool>(new FeaturesUsersCommands.DeleteUserCommand { UserId = userId }, "خطا در حذف کاربر");
            return NoContent();
        }

        /// <summary>
        /// فعال‌سازی حساب کاربری
        /// </summary>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateAccount(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");
            return await ExecuteCommand<bool>(new FeaturesUsersCommands.ActivateUserCommand { UserId = userId }, "خطا در فعال‌سازی حساب کاربری");
        }

        /// <summary>
        /// غیرفعال‌سازی حساب کاربری
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeactivateAccount(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");
            await ExecuteCommand<bool>(new FeaturesUsersCommands.DeactivateUserCommand { UserId = userId }, "خطا در غیرفعال‌سازی حساب کاربری");
            return Success("حساب کاربری با موفقیت غیرفعال شد");
        }
    }
}