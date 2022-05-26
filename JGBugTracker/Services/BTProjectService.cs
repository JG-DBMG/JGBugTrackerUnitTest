using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
    private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);
            }
            catch (Exception)
            {

                throw;
            }
        }

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

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project? project = new();

                 project = await _context.Projects
                                 .Include(p => p.Company)
                                 .Include(p => p.ProjectPriority)
                                 .FirstOrDefaultAsync(m => m.Id == projectId);
                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

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

    }
}
