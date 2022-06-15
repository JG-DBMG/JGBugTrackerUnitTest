using JGBugTracker.Data;
using JGBugTracker.Extensions;
using JGBugTracker.Models;
using JGBugTracker.Models.Enums;
using JGBugTracker.Models.ViewModels;
using JGBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JGBugTracker.Controllers
{ 
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              UserManager<BTUser> userManager,
                              IBTTicketService ticketService,
                              IBTRolesService rolesService,
                              IBTProjectService projectService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dashboard_new()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            model.Tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);
            model.Projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> teamMembers = developers.Concat(submitters).ToList();

            model.Members = teamMembers;

            return View(model);
        }
    }
}