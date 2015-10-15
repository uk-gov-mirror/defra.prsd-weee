﻿namespace EA.Weee.RequestHandlers.Admin.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Admin;
    using Core.Shared;
    using DataAccess;

    public class GetSubmissionsHistoryResultsDataAccess : IGetSubmissionsHistoryResultsDataAccess
    {
        private readonly WeeeContext context;

        public GetSubmissionsHistoryResultsDataAccess(WeeeContext context)
        {
            this.context = context;
        }

        public async Task<List<SubmissionsHistorySearchResult>> GetSubmissionsHistory(int year, Guid schemeId)
        {
        var results = await(from mu in context.MemberUploads
            join user in context.Users on mu.UserId equals user.Id
            where mu.IsSubmitted & mu.SchemeId == schemeId & mu.ComplianceYear == year
            select new SubmissionsHistorySearchResult
            {
                SchemeId = mu.SchemeId.Value,
                OrganisationId = mu.OrganisationId,
                MemberUploadId = mu.Id,
                SubmittedBy = user.FirstName + " " + user.Surname,
                Year = mu.ComplianceYear.Value,
                DateTime = mu.Date.Value,
                NoOfWarnings = (from me in context.MemberUploadErrors
                                where me.MemberUploadId == mu.Id & (me.ErrorLevel.Value == Domain.ErrorLevel.Warning.Value)
                                select me).Count()
            }).OrderByDescending(s => s.DateTime).ToListAsync();
        return results;
        }
    }
}