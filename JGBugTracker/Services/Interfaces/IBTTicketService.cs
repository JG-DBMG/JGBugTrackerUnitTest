using JGBugTracker.Models;

namespace JGBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
    }
}
