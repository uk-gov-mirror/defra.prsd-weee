﻿namespace EA.Weee.RequestHandlers.AatfReturn.CheckYourReturn
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess;
    using EA.Weee.Domain.AatfReturn;

    public class FetchNonObligatedWeeeForReturnDataAccess : IFetchNonObligatedWeeeForReturnDataAccess
    {
        private readonly WeeeContext context;

        public FetchNonObligatedWeeeForReturnDataAccess(WeeeContext context)
        {
            this.context = context;
        }

        public async Task<List<NonObligatedWeee>> FetchNonObligatedWeeeForReturn(Guid returnId)
        {
            return await context.NonObligatedWeee.Where(now => now.ReturnId == returnId).Select(now => now).ToListAsync();
        }

        public async Task<List<decimal?>> FetchNonObligatedWeeeForReturn(Guid returnId, bool dcf)
        {
            return await context.NonObligatedWeee.Where(now => now.ReturnId == returnId).Where(now => now.Dcf == dcf).Select(now => (decimal?)now.Tonnage).ToListAsync();
        }
    }
}