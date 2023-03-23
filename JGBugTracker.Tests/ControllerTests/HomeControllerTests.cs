using FakeItEasy;
using FluentAssertions;
using JGBugTracker.Controllers;
using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Services.Interfaces;
using JGBugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace JGBugTracker.Tests.ControllerTests
{
    public class HomeControllerTests
    {
        private readonly HomeController _homeController;
        private readonly ILogger<HomeController> _logger;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly DashboardViewModel _dashboardViewModel;

        public HomeControllerTests()
        {
            //Dependancies
            _context = A.Fake<ApplicationDbContext>();
            _companyInfoService = A.Fake<IBTCompanyInfoService>();
            _dashboardViewModel = A.Fake<DashboardViewModel>();
            _logger = A.Fake<ILogger<HomeController>>();
            _projectService = A.Fake<IBTProjectService>();
            _rolesService = A.Fake<IBTRolesService>();
            _ticketService = A.Fake<IBTTicketService>();
            _userManager = A.Fake<UserManager<BTUser>>();

            //SUT
            _homeController = new HomeController(_logger, _context, _userManager, _ticketService, _rolesService, _projectService, _companyInfoService);
        }

        [Fact]
        public void HomeController_Dashboard_Returns_Success()
        {
            //Arrange
            int companyId = 1;
            DashboardViewModel dashboardViewModel = A.Fake<DashboardViewModel>();

            //A.CallTo(() => _companyInfoService.GetCompanyInfoById(companyId)).Returns(dashboardViewModel);
            //A.CallTo(() => _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Returns(dashboardViewModel);
            //A.CallTo(() => _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Returns(dashboardViewModel);
            //A.CallTo(() => _companyInfoService.GetAllMembersAsync(companyId)).Returns(dashboardViewModel);

            //Act
            var result = _homeController.Dashboard();

            //Assert
            result.Should().BeOfType<Task<IActionResult>>();
        }
    }
}
