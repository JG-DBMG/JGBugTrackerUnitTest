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
using JGBugTracker.Models.Enums;
using System.Security.Claims;
using JGBugTracker.Models.ChartModels;
using JGBugTracker.Services;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Http;

namespace JGBugTracker.Tests.ControllerTests
{
    public class HomeControllerTests
    {
        private HomeController _homeController;
        private ILogger<HomeController> _logger;
        private IBTTicketService _ticketService;
        private IBTProjectService _projectService;
        private IBTCompanyInfoService _companyInfoService;
        private IBTRolesService _rolesService;
        private UserManager<BTUser> _userManager;

        public HomeControllerTests()
        {
            //Dependancies
            _companyInfoService = A.Fake<IBTCompanyInfoService>();
            _logger = A.Fake<ILogger<HomeController>>();
            _projectService = A.Fake<IBTProjectService>();
            _rolesService = A.Fake<IBTRolesService>();
            _ticketService = A.Fake<IBTTicketService>();
            _userManager = A.Fake<UserManager<BTUser>>();

            //SUT
            _homeController = new HomeController(_logger, _userManager, _ticketService, _rolesService, _projectService, _companyInfoService);
        }

        [Fact]
        public void Dashboard_Returns_DashboardViewModel()
        {
            //Arrange
            int companyId = 1;
            var dashboardViewModel = A.Fake<DashboardViewModel>();

            A.CallTo(() => _ticketService.GetAllTicketsByCompanyIdAsync(companyId))!.Returns(dashboardViewModel.Tickets);
            A.CallTo(() => _projectService.GetAllProjectsByCompanyIdAsync(companyId))!.Returns(dashboardViewModel.Projects);
            A.CallTo(() => _companyInfoService.GetAllMembersAsync(companyId))!.Returns(dashboardViewModel.Members);
            A.CallTo(() => _companyInfoService.GetCompanyInfoById(companyId))!.Returns(dashboardViewModel.Company);


            //Act
            var result = _homeController.Dashboard();

            //Assert
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public async Task PlotlyBarChart_Returns_PlotlyData()
        {
            // Arrange
            int companyId = 1;
            var projectService = A.Fake<IBTProjectService>();
            var userManager = A.Fake<UserManager<BTUser>>();

            //New controller
            var homeController = new HomeController(null!, userManager, null!, null!, projectService, null!);

            //New user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
                new Claim(ClaimTypes.Name, "user-name"),
                new Claim(ClaimTypes.Email, "user-email@example.com"),
                new Claim("companyId", companyId.ToString())
            }));

            //new context
            homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, Name = "Project 1", Tickets = new List<Ticket> { new Ticket(), new Ticket() } },
                new Project { Id = 2, Name = "Project 2", Tickets = new List<Ticket> { new Ticket() } }
            };

            A.CallTo(() => projectService.GetAllProjectsByCompanyIdAsync(companyId)).Returns(projects);
            A.CallTo(() => projectService.GetProjectMembersByRoleAsync(1, nameof(BTRoles.Developer))).Returns(new List<BTUser> { new BTUser(), new BTUser() });
            A.CallTo(() => projectService.GetProjectMembersByRoleAsync(2, nameof(BTRoles.Developer))).Returns(new List<BTUser> { new BTUser() });

            // Act
            var result = await homeController.PlotlyBarChart();
            var plotlyData = result.Value as PlotlyBarData;
            var barData = plotlyData!.Data;

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<JsonResult>();
            plotlyData.Should().NotBeNull();
            barData.Should().NotBeNull();
            barData.Should().HaveCount(2);
            barData![0].X.Should().BeEquivalentTo(new[] { "Project 1", "Project 2" });
            //barData[0].Y.Should().BeEquivalentTo(new[] { 2, 1 });
            barData[0].Name.Should().Be("Tickets");
            barData[1].X.Should().BeEquivalentTo(new[] { "Project 1", "Project 2" });
            barData[1].Y.Should().BeEquivalentTo(new[] { 2, 1 });
            barData[1].Name.Should().Be("Developers");
        }


    }
}
