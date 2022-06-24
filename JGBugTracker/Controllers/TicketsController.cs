using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JGBugTracker.Data;
using JGBugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JGBugTracker.Services.Interfaces;
using JGBugTracker.Extensions;
using JGBugTracker.Models.Enums;
using JGBugTracker.Models.ViewModels;

namespace JGBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTRolesService _rolesService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTProjectService projectService,
                                 IBTFileService fileService,
                                 IBTTicketHistoryService ticketHistoryService,
                                 IBTLookupService lookupService,
                                 IBTNotificationService notificationService,
                                 IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _fileService = fileService;
            _ticketHistoryService = ticketHistoryService;
            _lookupService = lookupService;
            _notificationService = notificationService;
            _rolesService = rolesService;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> AllTickets()
        {
            //BTUser btUser = await _userManager.GetUserAsync(User);
            //int companyId = btUser.CompanyId;
            int companyId = User.Identity!.GetCompanyId();
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            return View(tickets);
        }

        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AssignDeveloperViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            model.Developers = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName");

            return View(model);
            
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.Ticket!.DeveloperUserId != null)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                //oldTicket
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id);
                try
                {
                    await _ticketService.AssignTicketAsync(model.Ticket.Id, model.Ticket!.DeveloperUserId);
                }
                catch (Exception)
                {
                    throw;
                }
                //newTicket
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);
                // Add History
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);
                //Send Notifications
                //Notify Developer
                if (model.Ticket.DeveloperUserId != null)
                {
                    Notification devNotification = new()
                    {
                        TicketId = model.Ticket.Id,
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationTypes.Ticket))).Value,
                        Title = "Ticket Updated",
                        Message = $"Ticket: {model.Ticket.Title}, was updated by {btUser.FullName}",
                        Created = DateTime.UtcNow,
                        SenderId = btUser.Id,
                        RecipientId = model.Ticket.DeveloperUserId
                    };
                    await _notificationService.AddNotificationAsync(devNotification);
                    await _notificationService.SendEmailNotificationAsync(devNotification, "Ticket Updated");
                }

                return RedirectToAction(nameof(Details), new { id = model.Ticket?.Id });
            }
            return RedirectToAction(nameof(AssignDeveloper));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }
            
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.UtcNow;
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == "New"))!.Id;

                await _ticketService.AddNewTicketAsync(ticket);

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistoryService.AddHistoryAsync(null!, newTicket, ticket.SubmitterUserId);

                return RedirectToAction(nameof(AllTickets));
            }
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            //BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
            //Notification notification = new()
            //{
            //    NotificationTypeId = (await _lookupService.LookupNotificationTypeId(nameof(BTNotificationType.Ticket))).Value,
            //    TicketId = ticket.Id,
            //    Title = "New Ticket Added",
            //    Message = $"New Ticket: {ticket.Title}, was created by {btUser.FullName}",
            //    Created = DateTime.UtcNow,
            //    SenderId = userId,
            //    RecipientId = projectManager?.Id
            //};

            //await _notificationService.AddNotificationAsync(notification);
            //if (projectManager != null)
            //{
            //    await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added For Project: {newTicket.Project.Name}");
            //}
            //else
            //{
            //    await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, nameof(BTRoles.Admin));
            //}

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,SubmitterUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //TODO: Add History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);


                return RedirectToAction(nameof(AllTickets));
            }
            
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Ticket> tickets = await _ticketService.GetAllArchivedTicketsByCompanyIdAsync(companyId);
            return View(tickets);
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket != null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }

            return RedirectToAction(nameof(AllTickets));
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }
            
            return RedirectToAction(nameof(AllTickets));
        }

        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId,companyId);
            return View(tickets);
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);
            return View(tickets);
        }
    }
}
