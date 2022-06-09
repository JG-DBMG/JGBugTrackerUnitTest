﻿using JGBugTracker.Data;
using JGBugTracker.Models;
using JGBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JGBugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            try
            {
                List<BTUser> members = new();
                members = (await _context.Companies.Include(c => c.Members).FirstOrDefaultAsync(m => m.Id == companyId))!.Members.ToList();
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Company> GetCompanyInfoById(int? companyId)
        {
            try
            {
                Company? company = new();

                if (companyId != null)
                {
                    company = await _context.Companies
                                            .Include(c => c.Members)
                                            .Include(c => c.Projects)
                                            .Include(c => c.Invites)
                                            .FirstOrDefaultAsync(m => m.Id == companyId);
                }
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}