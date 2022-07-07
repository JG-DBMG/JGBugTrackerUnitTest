using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Models.Enums;
using JGBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        #region Properties
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService; 
        #endregion

        #region Constructors
        public BTTicketService(ApplicationDbContext context,
                               IBTRolesService rolesService,
                               IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        } 
        #endregion

        #region Add New Ticket
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
        #endregion

        #region Add Ticket Attachment
        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Archive Ticket
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

        #endregion

        #region Add Ticket Comment
        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        {
            try
            {
                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        } 
        #endregion

        #region Assign Ticket
        public async Task AssignTicketAsync(int ticketId, string DeveloperId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

                ticket!.DeveloperUserId = DeveloperId;
                ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                ticket.Updated = DateTime.UtcNow;

                await UpdateTicketAsync(ticket);

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Tickets As No Tracking
        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets
                                     .Include(t => t.DeveloperUser)
                                     .Include(t => t.Project)
                                     .Include(t => t.TicketPriority)
                                     .Include(t => t.TicketStatus)
                                     .Include(t => t.TicketType)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(t => t.Id == ticketId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Get All Tickets By Id
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

        #endregion

        #region Get All Archived Tickets By Company Id
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

        #endregion

        #region Get Tickets By Company Id
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
        #endregion

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
                                                    .SelectMany(p => p.Tickets!.Where(p => p.Archived == false)).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Developer)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!.Where(p => p.Archived == false))
                                                    .Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Submitter)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(t => t.Tickets!).Where(t => t.SubmitterUserId == userId && t.Archived == false).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
                {
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets!).ToList();
                    List<Ticket>? submittedTickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!.Where(p => p.Archived == false)).Where(t => t.SubmitterUserId == userId).ToList();
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

        #region Get Ticket Attachment By Id
        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Ticket By Id
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
        #endregion

        #region Get Unassigned Tickets
        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();
                tickets = await _context.Projects.Where(t => t.CompanyId == companyId)
                                                 .SelectMany(t => t.Tickets.Where(t => t.Archived == false && t.DeveloperUser == null))
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser)
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

        #endregion

        #region Restore Ticket
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

        #endregion

        #region Update Ticket
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

        #endregion    
    }
}
