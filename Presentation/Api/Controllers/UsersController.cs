using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر مدیریت کاربران
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : BaseApiController
    {
        /// <summary>
        /// سازنده کنترلر
        /// </summary>
        public UsersController(
            IMediator mediator,
            ILogger<UsersController> logger) : base(logger, mediator)
        {
        }

        #region مدیریت کاربران
        /// <summary>
        /// دریافت اطلاعات کاربر با شناسه
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>اطلاعات کاربر</returns>
        /// <response code="200">دریافت موفق اطلاعات کاربر</response>
        /// <response code="400">شناسه کاربر نامعتبر است</response>
        /// <response code="404">کاربر یافت نشد</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            return await ExecuteCommand(new GetUserByIdQuery { UserId = userId });
        }

        /// <summary>
        /// دریافت لیست تمام کاربران
        /// </summary>
        /// <returns>لیست کاربران</returns>
        /// <response code="200">دریافت موفق لیست کاربران</response>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(PaginatedList<UserDto>), 200)]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetUsersQuery query)
        {
            return await ExecuteCommand(query);
        }

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        /// <param name="command">اطلاعات کاربر جدید</param>
        /// <returns>اطلاعات کاربر ایجاد شده</returns>
        /// <response code="201">کاربر با موفقیت ایجاد شد</response>
        /// <response code="400">اطلاعات کاربر نامعتبر است</response>
        /// <response code="401">دسترسی غیرمجاز</response>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            var result = await ExecuteCommand(command, "خطا در ایجاد کاربر");
            if (result is OkObjectResult okResult)
            {
                var userResponse = (UserDto)okResult.Value;
                return CreatedAtAction(nameof(GetUserById), new { id = userResponse.Id }, userResponse);
            }
            return result;
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <param name="command">اطلاعات جدید کاربر</param>
        /// <returns>اطلاعات به‌روز شده کاربر</returns>
        /// <response code="200">به‌روزرسانی موفق اطلاعات کاربر</response>
        /// <response code="400">اطلاعات نامعتبر است</response>
        /// <response code="401">دسترسی غیرمجاز</response>
        /// <response code="404">کاربر یافت نشد</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            command.UserId = userId;
            return await ExecuteCommand(command, "خطا در به‌روزرسانی کاربر");
        }

        /// <summary>
        /// حذف کاربر
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>نتیجه عملیات حذف</returns>
        /// <response code="204">حذف موفق کاربر</response>
        /// <response code="400">شناسه کاربر نامعتبر است</response>
        /// <response code="401">دسترسی غیرمجاز</response>
        /// <response code="404">کاربر یافت نشد</response>
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

            await ExecuteCommand(new DeleteUserCommand { UserId = userId }, "خطا در حذف کاربر");
            return NoContent();
        }

        /// <summary>
        /// فعال‌سازی حساب کاربری
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        /// <response code="200">حساب کاربری با موفقیت فعال شد</response>
        /// <response code="400">شناسه کاربر نامعتبر است</response>
        /// <response code="404">کاربر یافت نشد</response>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateAccount(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            return await ExecuteCommand(new ActivateUserCommand { UserId = userId }, "خطا در فعال‌سازی حساب کاربری");
        }

        /// <summary>
        /// غیرفعال‌سازی حساب کاربری
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        /// <response code="200">حساب کاربری با موفقیت غیرفعال شد</response>
        /// <response code="400">شناسه کاربر نامعتبر است</response>
        /// <response code="404">کاربر یافت نشد</response>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeactivateAccount(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            await _mediator.Send(new DeactivateUserCommand { UserId = userId });
            return Success("حساب کاربری با موفقیت غیرفعال شد");
        }
        #endregion
    }
}