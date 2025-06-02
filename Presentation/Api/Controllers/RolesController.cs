using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.Exceptions;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleManagementService _roleService;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleManagementService roleService,
            IUserAuthorizationService userAuthorizationService,
            ILogger<RolesController> logger)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _userAuthorizationService = userAuthorizationService ?? throw new ArgumentNullException(nameof(userAuthorizationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Role Management

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), 200)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست نقش‌ها");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RoleDto>> GetRoleById(Guid id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                return Ok(role);
            }
            catch (RoleNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اطلاعات نقش");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto request)
        {
            try
            {
                var role = await _roleService.CreateRoleAsync(request);
                return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
            }
            catch (DuplicateRoleException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد نقش جدید");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, [FromBody] UpdateRoleDto request)
        {
            try
            {
                var role = await _roleService.UpdateRoleAsync(id, request);
                return Ok(role);
            }
            catch (RoleNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در به‌روزرسانی نقش");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            try
            {
                await _roleService.DeleteRoleAsync(id);
                return Ok(new { message = "نقش با موفقیت حذف شد" });
            }
            catch (RoleNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف نقش");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        #endregion

        #region Permission Management

        [HttpPost("{roleId}/permissions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> AssignPermissions(Guid roleId, [FromBody] AssignPermissionsRequest request)
        {
            try
            {
                await _roleService.AssignPermissionsToRoleAsync(roleId, request.PermissionIds);
                return Ok(new { message = "دسترسی‌ها با موفقیت به نقش اختصاص داده شد" });
            }
            catch (RoleNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اختصاص دسترسی‌ها به نقش");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpDelete("{roleId}/permissions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RemovePermissions(Guid roleId, [FromBody] RemovePermissionsRequest request)
        {
            try
            {
                await _roleService.RemovePermissionsFromRoleAsync(roleId, request.PermissionIds);
                return Ok(new { message = "دسترسی‌ها با موفقیت از نقش حذف شد" });
            }
            catch (RoleNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف دسترسی‌ها از نقش");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        #endregion

        #region User-Role Management

        [HttpPost("users/{userId}/roles/{roleName}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> AssignRoleToUser(string userId, string roleName)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            try
            {
                var result = await _userAuthorizationService.AssignRoleAsync(userGuid, roleName);
                if (!result)
                    return BadRequest("نقش با موفقیت به کاربر اختصاص داده نشد");

                return Ok(new { message = "نقش با موفقیت به کاربر اختصاص داده شد" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اختصاص نقش به کاربر");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpDelete("users/{userId}/roles/{roleName}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RemoveRoleFromUser(string userId, string roleName)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            try
            {
                var result = await _userAuthorizationService.RemoveRoleAsync(userGuid, roleName);
                if (!result)
                    return BadRequest("نقش با موفقیت از کاربر حذف نشد");

                return Ok(new { message = "نقش با موفقیت از کاربر حذف شد" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف نقش از کاربر");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
        }

        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            try
            {
                var roles = await _userAuthorizationService.GetUserRolesAsync(userGuid);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت نقش‌های کاربر");
                return StatusCode(500, new { error = "خطای داخلی سرور" });
            }
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