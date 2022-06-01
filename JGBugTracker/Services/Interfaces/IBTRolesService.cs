using JGBugTracker.Models;

namespace JGBugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);

        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
    }
}
