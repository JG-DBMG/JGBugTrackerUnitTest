﻿using JGBugTracker.Models;

namespace JGBugTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {
        public Task<List<BTUser>> GetAllMembersAsync(int companyId);

        public Task<Company> GetCompanyInfoById(int? companyId);
    }
}
