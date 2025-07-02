using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.PermissionManagement;

namespace UserManagement.Application.PermissionManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionAppService : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionAppService(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Get all permissions in your tenant.
        /// </summary>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Create a new permission (page permission) for your tenant.
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<PermissionDto>> CreatePermission(CreatePermissionDto dto)
        {
            var permission = await _permissionService.CreatePermissionAsync(dto);
            return CreatedAtAction(nameof(GetAllPermissions), new { id = permission.Id }, permission);
        }

        /// <summary>
        /// Assign a permission to a role.
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignPermissionToRole(AssignPermissionDto dto)
        {
            await _permissionService.AssignPermissionToRoleAsync(dto.RoleId, dto.PermissionId);
            return NoContent();
        }

        /// <summary>
        /// Remove a permission from a role.
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete("remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemovePermissionFromRole(RemovePermissionDto dto)
        {
            await _permissionService.RemovePermissionFromRoleAsync(dto.RoleId, dto.PermissionId);
            return NoContent();
        }

        /// <summary>
        /// Get permissions assigned to a role.
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("role/{roleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetPermissionsByRole(int roleId)
        {
            var permissions = await _permissionService.GetPermissionsByRoleAsync(roleId);
            return Ok(permissions);
        }

        /// <summary>
        /// Get granted permissions for the logged-in user
        /// </summary>
        ///
        [Authorize]
        [HttpGet("granted")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetGrantedPermissionsForUser()
        {
            var permissions = await _permissionService.GetGrantedPermissionsForLoggedInUserAsync();
            return Ok(permissions);
        }
    }
}