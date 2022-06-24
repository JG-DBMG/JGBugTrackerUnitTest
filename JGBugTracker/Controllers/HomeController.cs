using JGBugTracker.Data;
using JGBugTracker.Extensions;
using JGBugTracker.Models;
using JGBugTracker.Models.ChartModels;
using JGBugTracker.Models.Enums;
using JGBugTracker.Models.ViewModels;
using JGBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IBTCompanyInfoService _companyInfoService;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              UserManager<BTUser> userManager,
                              IBTTicketService ticketService,
                              IBTRolesService rolesService,
                              IBTProjectService projectService,
                              IBTCompanyInfoService companyInfoService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            model.Tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);
            model.Projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            model.Company = await _companyInfoService.GetCompanyInfoById(companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> teamMembers = developers.Concat(submitters).ToList();

            model.Members = teamMembers;

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> PlotlyBarChart()
        {
            PlotlyBarData plotlyData = new();
            List<PlotlyBar> barData = new();

            int companyId = User.Identity.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            //Bar One
            PlotlyBar barOne = new()
            {
                X = projects.Select(p => p.Name).ToArray(),
                Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
                Name = "Tickets",
                Type = "bar"
            };

            //Bar Two
            PlotlyBar barTwo = new()
            {
                X = projects.Select(p => p.Name).ToArray(),
                Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(BTRoles.Developer))).Count).Select(c => c.Result).ToArray(),
                Name = "Developers",
                Type = "bar"
            };

            barData.Add(barOne);
            barData.Add(barTwo);

            plotlyData.Data = barData;

            return Json(plotlyData);
        }
    }
}