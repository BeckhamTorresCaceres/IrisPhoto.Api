using IrisPhoto_Api.Domain.Entities;
using IrisPhoto_Api.Domain.Enums;
using IrisPhoto_Api.Models.Common;
using IrisPhoto_Api.Models.Users;
using IrisPhoto_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IrisPhoto_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class BoxesController : ControllerBase
    {
        IBoxesServices _boxesServices;
        public BoxesController(IBoxesServices boxesServices)
        {
            _boxesServices = boxesServices;
        }

        [HttpPost("Create")]
        public async Task<ActionResult> Create(Box box)
        {
            var userId = User.GetUserId();

            box.UserId = userId;

            await _boxesServices.AddAsync(box);

            return Ok(ApiResponse<Box>.Ok(box, "element created successfully"));
        }

        [HttpPost("Get")]
        public async Task<ActionResult> Get(PasswordType passwordType, string password)
        {
            var userId = User.GetUserId();

            var box =  _boxesServices.GetAll().FirstOrDefault(
                x => x.UserId == userId &&
                x.PasswordType == passwordType &&
                x.Password == password &&
                x.IsActive == true
            );

            if (box is null)
            {
                return NotFound(ApiResponse<Box>.Fail("Element not found"));
            }

            return Ok(ApiResponse<Box>.Ok(box));
        }
    }
}
