﻿using JGBugTracker.Models;

namespace JGBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddNewTicketAsync(Ticket ticket);

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task AddTicketCommentAsync(TicketComment ticketComment);

        public Task ArchiveTicketAsync(Ticket ticket);

        public Task AssignTicketAsync(int ticketId, string DeveloperId);

        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId);

        public Task<List<Ticket>> GetAllTicketsByIdAsync(int ticketId);

        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        public Task<List<Ticket>> GetAllArchivedTicketsByCompanyIdAsync(int companyId);

        public Task<Ticket> GetTicketByIdAsync(int ticketId);

        public Task<List<Ticket>> GetUnassignedTicketsAsync(int ticketId);

        public Task RestoreTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);
    }
}
