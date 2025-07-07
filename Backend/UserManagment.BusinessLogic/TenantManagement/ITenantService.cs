using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;

namespace UserManagement.BusinessLogic.TenantManagement
{
    public interface ITenantService
    {
        Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
        Task<List<TenantDto>> GetAllTenantsAsync();
        Task<TenantDto> GetTenantByIdAsync(long id);
        Task UpdateTenantAsync(long id, UpdateTenantDto dto);
        Task DeleteTenantAsync(long id);
    }

}
