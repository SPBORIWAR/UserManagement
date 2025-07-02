using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.TenantManagement;

namespace UserManagement.Application.TenantManagement
{
    [Authorize(Roles = "SuperAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    
    //[Authorize(Roles = "Admin")] // Only admins can manage tenants
    public class TenantAppService : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantAppService> _logger;

        public TenantAppService(ITenantService tenantService, ILogger<TenantAppService> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new tenant with default admin and user.
        /// </summary>
        [HttpPost("create")]
        [SwaggerOperation(Summary = "Create a new tenant")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
        {
            _logger.LogInformation("Creating tenant: {TenantName}", dto.Name);
            var result = await _tenantService.CreateTenantAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Gets all tenants.
        /// </summary>
        [HttpGet("all")]
        [SwaggerOperation(Summary = "Get all tenants")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all tenants");
            var result = await _tenantService.GetAllTenantsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Gets a tenant by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get tenant by ID")]
        public async Task<IActionResult> GetById(long id)
        {
            _logger.LogInformation("Retrieving tenant with ID: {TenantId}", id);
            var result = await _tenantService.GetTenantByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Updates a tenant.
        /// </summary>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update tenant")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateTenantDto dto)
        {
            _logger.LogInformation("Updating tenant with ID: {TenantId}", id);
            await _tenantService.UpdateTenantAsync(id, dto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a tenant.
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete tenant")]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogWarning("Deleting tenant with ID: {TenantId}", id);
            await _tenantService.DeleteTenantAsync(id);
            return NoContent();
        }
    }
}
