using JGBugTracker.Data;
using JGBugTracker.Extensions;
using JGBugTracker.Models;
using JGBugTracker.Models.ViewModels;
using JGBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Controllers
{


    [Authorize(Roles = "Admin")]
    public class UserRolesController : Controller
    {
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly ApplicationDbContext _context;

        public UserRolesController(IBTCompanyInfoService companyInfoService,
                                   IBTRolesService rolesService,
                                   ApplicationDbContext context)
        {
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Users()

        {
            int companyId = User.Identity!.GetCompanyId();

            var company = await _context.Companies
                                        .Include(m => m.Members)
                                        .FirstOrDefaultAsync(m => m.Id == companyId);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }


        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            int companyId = User.Identity!.GetCompanyId();
            List<BTUser> btUsers = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser btUser in btUsers)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BTUser = btUser;
                IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetBTRolesAsync(), "Name", "Name", currentRoles);

                model.Add(viewModel);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity!.GetCompanyId();
            BTUser? btUser = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.BTUser!.Id);

            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser!);

            string? selectedUserRole = member.SelectedRoles!.FirstOrDefault();

            if (!string.IsNullOrEmpty(selectedUserRole))
            {
                if (await _rolesService.RemoveUserFromRolesAsync(btUser!, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(btUser!, selectedUserRole);
                }
            }
            else
            {
                return RedirectToAction(nameof(ManageUserRoles));
            }
            return RedirectToAction("ManageUserRoles", "UserRoles");
        }
    }
}
