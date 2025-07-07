using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.RoleManagement;

namespace UserManagement.Application.RoleManagement
{    
    [Route("api/[controller]")]
    [ApiController]
    public class RoleAppService : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleAppService(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRoleById(long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto dto)
        {
            var role = await _roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateRole(RoleDto dto)
        {
            await _roleService.UpdateRoleAsync(dto);
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto dto)
        {
            await _roleService.AssignRoleToUserAsync(dto.UserId, dto.RoleId);
            return Ok("Role assigned to user successfully.");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignRoleDto dto)
        {
            await _roleService.RemoveRoleFromUserAsync(dto.UserId, dto.RoleId);
            return Ok("Role removed from user successfully.");
        }

        [Authorize]
        [HttpGet("user/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RoleDto>>> GetRolesByUserId(long userId)
        {
            var roles = await _roleService.GetRolesByUserIdAsync(userId);
            return Ok(roles);
        }

        [Authorize]
        [HttpGet("user/{userId}/primary-role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleDto>> GetPrimaryRoleByUserId(long userId)
        {
            var primaryRole = await _roleService.GetPrimaryRoleByUserIdAsync(userId);
            return Ok(primaryRole);
        }


    }


}
