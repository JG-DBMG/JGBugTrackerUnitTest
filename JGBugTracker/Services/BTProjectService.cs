using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Models.Enums;
using JGBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        #region Properties
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService; 
        #endregion

        #region Constructor
        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        } 
        #endregion

        #region Add New Project
        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Add Project Manager
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    //Remove Project Managers
                    await RemoveProjectManagerAsync(projectId);

                }
                catch (Exception)
                {

                    throw;
                }
            }
            // Add the new PM
            try
            {
                //Add Project Manager
                return await AddUserToProjectAsync(userId, projectId);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Add User To Project
        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (btUser != null)
            {
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                if (!await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project!.Members!.Add(btUser);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Archive Project
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                //Archive project tickets
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Project Members By Role
        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName)
        {
            try
            {
                Project? project = await _context.Projects
                                                    .Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId);
                List<BTUser> members = new();

                foreach (BTUser user in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(user, roleName))
                    {
                        members.Add(user);
                    }
                }
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Users Not On Project
        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            try
            {
                List<BTUser> users = await _context.Users.Where(u => u.Projects!.All(p => p.ProjectPriorityId != projectId) && u.CompanyId == companyId).ToListAsync();
                return users;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get All Project Members Except PM
        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            try
            {
                List<BTUser> developers = await GetProjectMembersByRoleAsync(projectId, nameof(BTRoles.Developer));
                List<BTUser> submitters = await GetProjectMembersByRoleAsync(projectId, nameof(BTRoles.Submitter));
                List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, nameof(BTRoles.Admin));

                List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

                return teamMembers;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Archived Projects By Company Id
        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project>? projects = new();

                projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                                  .Include(p => p.Members)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.SubmitterUser)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get All Projects By Company Id
        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project>? projects = new();

                projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.SubmitterUser)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
            //var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.Priority).Where(p => p.CompanyId == companyId);
            //return await applicationDbContext.ToListAsync();
        }
        #endregion

        #region Get Unassigned Projects
        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();
            try
            {
                projects = await _context.Projects
                                         .Include(p => p.ProjectPriority)
                                         .Include(p => p.Members)
                                         .Where(p => p.CompanyId == companyId && p.Archived == false).ToListAsync();
                foreach (Project project in projects)
                {
                    if ((await GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.ProjectManager))).Count == 0)
                    {
                        result.Add(project);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Get User Projects
        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Company)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Members)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(p => p.DeveloperUser)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(p => p.SubmitterUser)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(p => p.TicketPriority)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(p => p.TicketStatus)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(p => p.TicketType)
                                                         .FirstOrDefaultAsync(u => u.Id == userId))?.Projects!.Where(u => u.Archived == false).ToList();
                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Project By Id
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = new();

                project = await _context.Projects.Where(p => p.Archived == false)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.SubmitterUser)
                                            .Include(p => p.Members)
                                            .Include(p => p.ProjectPriority)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        } 
        #endregion

        #region Get Project Manager
        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project?.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Is User On Project
        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);
                bool result = false;

                if (project != null)
                {
                    result = project.Members.Any(m => m.Id == userId);
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        } 
        #endregion

        #region Remove Users From Project
        public async Task<bool> RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        project?.Members?.Remove(btUser!);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    return false;

                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Remove Project Manager
        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser member in project?.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Restore Project
        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                //Restore project tickets
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                };
            }
            catch (Exception)
            {

                throw;
            }
        } 
        #endregion

        #region Update Project
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        } 
        #endregion

    }
}
