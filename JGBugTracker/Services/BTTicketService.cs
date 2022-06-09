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

        public BTTicketService(ApplicationDbContext context, 
                               IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
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
