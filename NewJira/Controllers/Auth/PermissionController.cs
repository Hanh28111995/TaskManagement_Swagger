using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;

namespace NewJira.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepo;

        public PermissionController(IPermissionRepository permissionRepo)
            => _permissionRepo = permissionRepo;

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionRepo.GetAllAsync();
            return Ok(new ResponseResultSuccess<object>("Danh sách quyền", permissions));
        }

        [Authorize(Policy = "role.assign")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PermissionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new ResponseResultError<object>("Code quyền không được để trống!"));

            var permission = new Permission { Code = dto.Code, Description = dto.Description };
            await _permissionRepo.AddAsync(permission);
            return Ok(new ResponseResultSuccess<object>("Tạo quyền thành công", permission));
        }
    }

    public class PermissionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
