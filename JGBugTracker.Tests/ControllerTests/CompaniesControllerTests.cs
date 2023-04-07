using FakeItEasy;
using FluentAssertions;
using JGBugTracker.Controllers;
using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JGBugTracker.Tests.ControllerTests
{
    public class CompaniesControllerTests
    {
        #region Dependancies
        private IBTCompanyInfoService _companyInfoService;

        public CompaniesControllerTests()
        {
            _companyInfoService = A.Fake<IBTCompanyInfoService>();
        } 
        #endregion

        #region MockUps
        private async Task<(ApplicationDbContext, ControllerContext)> GetDbContext()
        {
            // create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "TestUser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // create controller context with the claims principal
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Companies.CountAsync() < 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    databaseContext.Companies.Add(
                        new Company()
                        {
                            Id = 1,
                            Name = "New Company",
                            Description = "Description",
                            Projects = new List<Project>(),
                            Members = new List<BTUser>(),
                            Invites = new List<Invite>()
                        });
                    await databaseContext.SaveChangesAsync();
                }
            }
            return (databaseContext, controllerContext);
        }
        #endregion


        #region HappyPath
        [Fact]
        public async void Index_Returns_IActionResult()
        {
            // Arrange
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = companiesController.Index();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public async void Details_Returns_IActionResult()
        {
            // Arrange
            var id = 1;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = companiesController.Details(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public async void Edit_Returns_IActionResult()
        {
            // Arrange
            var id = 1;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = companiesController.Edit(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public async void Create_Returns_IActionResult()
        {
            // Arrange
            var (dbContext, controllerContext) = await GetDbContext();
            var company = A.Fake<Company>();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = companiesController.Create(company);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public async void Delete_Returns_IActionResult()
        {
            // Arrange
            var id = 1;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = companiesController.Delete(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public async void DeleteConfirmed_Returns_IActionResult()
        {
            // Arrange
            var id = 1;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = companiesController.DeleteConfirmed(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<IActionResult>>();
        }
        #endregion

        #region SadPath
        [Fact]
        public async void Details_NullIdReturns_NotFound()
        {
            // Arrange
            int? id = null;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = await companiesController.Details(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async void Details_NullCompanyReturns_NotFound()
        {
            // Arrange
            int id = 1;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = await companiesController.Details(id + 1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async void Edit_NullIdReturns_NotFound()
        {
            // Arrange
            int? id = null;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = await companiesController.Edit(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async void Edit_NullCompanyReturns_NotFound()
        {
            // Arrange
            int id = 1;
            var (dbContext, controllerContext) = await GetDbContext();
            var companiesController = new CompaniesController(dbContext, _companyInfoService)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = await companiesController.Edit(id + 1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
        #endregion
    }
}
