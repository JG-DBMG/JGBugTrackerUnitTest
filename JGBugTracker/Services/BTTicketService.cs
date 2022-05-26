using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            List<Ticket> tickets = new List<Ticket>();
            tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                             .SelectMany(p => p.Tickets)
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
            //Second Solution
            //List<Ticket> bTickets = await _context.Tickets.Include(t => t.Project).Where(t => t.Project.CompanyId == companyId).ToListAsync();
                                                       
            return tickets;
        }
    }
}
