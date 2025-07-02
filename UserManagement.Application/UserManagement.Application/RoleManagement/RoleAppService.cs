using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.RoleManagement;

namespace UserManagement.Application.RoleManagement
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleAppService : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleAppService(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRoleById(long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto dto)
        {
            var role = await _roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRole(RoleDto dto)
        {
            await _roleService.UpdateRoleAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
    }


}
