using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Authorization_Login_Asp.Net.Application.Interfaces;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.API.Controllers
{
    /// <summary>
    /// کنترلر مدیریت کاربران
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// سازنده کنترلر
        /// </summary>
        /// <param name="userService">سرویس مدیریت کاربران</param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// دریافت اطلاعات کاربر با شناسه
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>اطلاعات کاربر</returns>
        /// <response code="200">دریافت موفق اطلاعات کاربر</response>
        /// <response code="400">شناسه کاربر نامعتبر است</response>
        /// <response code="404">کاربر یافت نشد</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// دریافت لیست تمام کاربران
        /// </summary>
        /// <returns>لیست کاربران</returns>
        /// <response code="200">دریافت موفق لیست کاربران</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        /// <param name="request">اطلاعات کاربر جدید</param>
        /// <returns>اطلاعات کاربر ایجاد شده</returns>
        /// <response code="201">کاربر با موفقیت ایجاد شد</response>
        /// <response code="400">اطلاعات کاربر نامعتبر است</response>
        /// <response code="401">دسترسی غیرمجاز</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <param name="request">اطلاعات جدید کاربر</param>
        /// <returns>اطلاعات به‌روز شده کاربر</returns>
        /// <response code="200">به‌روزرسانی موفق اطلاعات کاربر</response>
        /// <response code="400">اطلاعات نامعتبر است</response>
        /// <response code="401">دسترسی غیرمجاز</response>
        /// <response code="404">کاربر یافت نشد</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(userId, request);
            if (user == null)
                return NotFound();

            return Ok(user);
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
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var result = await _userService.DeleteUserAsync(userId);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
} 