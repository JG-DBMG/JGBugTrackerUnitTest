using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Models.Enums;
using JGBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context,
                               IBTRolesService rolesService,
                               IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByIdAsync(int ticketId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();

                tickets = await _context.Tickets.Where(m => m.Id == ticketId)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.History)
                .ToListAsync();

                return tickets!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllArchivedTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();
                tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                 .SelectMany(p => p.Tickets.Where(t => t.Archived == true))
                                                    .Include(p => p.Attachments)
                                                    .Include(p => p.Comments)
                                                    .Include(p => p.DeveloperUser)
                                                    .Include(p => p.History)
                                                    .Include(p => p.SubmitterUser)
                                                    .Include(p => p.TicketPriority)
                                                    .Include(p => p.TicketStatus)
                                                    .Include(p => p.TicketType)
                                                    .Include(p => p.Project)
                                                 .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();
                tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                 .SelectMany(p => p.Tickets.Where(t => t.Archived == false))
                                                    .Include(p => p.Attachments)
                                                    .Include(p => p.Comments)
                                                    .Include(p => p.DeveloperUser)
                                                    .Include(p => p.History)
                                                    .Include(p => p.SubmitterUser)
                                                    .Include(p => p.TicketPriority)
                                                    .Include(p => p.TicketStatus)
                                                    .Include(p => p.TicketType)
                                                    .Include(p => p.Project)
                                                 .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Get Tickets By User Id
        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket>? tickets = new();

            try
            {
                if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Admin)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Developer)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!)
                                                    .Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Submitter)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(t => t.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
                {
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets!).ToList();
                    List<Ticket>? submittedTickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();
                    tickets = projectTickets.Concat(submittedTickets).ToList();
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = new();

                ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.History)
                .FirstOrDefaultAsync(m => m.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();
                tickets = await _context.Projects.Where(t => t.CompanyId == companyId)
                                                 .SelectMany(t => t.Tickets.Where(t => t.Archived == false))
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser!.Id == null)
                                                    .Include(t => t.History)
                                                    .Include(t => t.SubmitterUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                 .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
