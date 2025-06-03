using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.Exceptions;
using Authorization_Login_Asp.Net.Core.Application.Features.Roles.Commands;
using Authorization_Login_Asp.Net.Core.Application.Features.Roles.Queries;
using MediatR;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر مدیریت نقش‌ها و دسترسی‌ها
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RolesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public RolesController(
            IMediator mediator,
            ILogger<RolesController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        #region مدیریت نقش‌ها
        /// <summary>
        /// دریافت لیست تمام نقش‌ها
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(GetRolesResponse), 200)]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _mediator.Send(new GetRolesQuery());
            return Success(result);
        }

        /// <summary>
        /// دریافت اطلاعات نقش با شناسه
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetRoleByIdResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var result = await _mediator.Send(new GetRoleByIdQuery { RoleId = id });
            return Success(result);
        }

        /// <summary>
        /// ایجاد نقش جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateRoleResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetRoleById), new { id = result.Id }, result);
        }

        /// <summary>
        /// به‌روزرسانی نقش
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateRoleResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.RoleId = id;
            var result = await _mediator.Send(command);
            return Success(result);
        }

        /// <summary>
        /// حذف نقش
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            await _mediator.Send(new DeleteRoleCommand { RoleId = id });
            return NoContent();
        }
        #endregion

        #region مدیریت دسترسی‌ها
        /// <summary>
        /// اختصاص دسترسی‌ها به نقش
        /// </summary>
        [HttpPost("{roleId}/permissions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignPermissions(Guid roleId, [FromBody] AssignPermissionsCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.RoleId = roleId;
            await _mediator.Send(command);
            return Success("دسترسی‌ها با موفقیت به نقش اختصاص داده شد");
        }

        /// <summary>
        /// حذف دسترسی‌ها از نقش
        /// </summary>
        [HttpDelete("{roleId}/permissions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemovePermissions(Guid roleId, [FromBody] RemovePermissionsCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.RoleId = roleId;
            await _mediator.Send(command);
            return Success("دسترسی‌ها با موفقیت از نقش حذف شد");
        }
        #endregion

        #region مدیریت نقش‌های کاربر
        /// <summary>
        /// اختصاص نقش به کاربر
        /// </summary>
        [HttpPost("users/{userId}/roles/{roleName}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
                return Error("شناسه کاربر نامعتبر است");

            await _mediator.Send(new AssignRoleToUserCommand 
            { 
                UserId = userGuid,
                RoleName = roleName
            });
            return Success("نقش با موفقیت به کاربر اختصاص داده شد");
        }

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        [HttpDelete("users/{userId}/roles/{roleName}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
                return Error("شناسه کاربر نامعتبر است");

            await _mediator.Send(new RemoveRoleFromUserCommand 
            { 
                UserId = userGuid,
                RoleName = roleName
            });
            return Success("نقش با موفقیت از کاربر حذف شد");
        }

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(typeof(GetUserRolesResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
                return Error("شناسه کاربر نامعتبر است");

            var result = await _mediator.Send(new GetUserRolesQuery { UserId = userGuid });
            return Success(result);
        }
        #endregion
    }

    public class AssignPermissionsRequest
    {
        public IEnumerable<Guid> PermissionIds { get; set; }
    }

    public class RemovePermissionsRequest
    {
        public IEnumerable<Guid> PermissionIds { get; set; }
    }
} 