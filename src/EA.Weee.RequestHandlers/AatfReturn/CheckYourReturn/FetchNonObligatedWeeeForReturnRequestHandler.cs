﻿namespace EA.Weee.RequestHandlers.AatfReturn.CheckYourReturn
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EA.Prsd.Core.Mediator;
    using Security;
    using Request = Requests.AatfReturn.NonObligated.FetchNonObligatedWeeeForReturnRequest;

    public class FetchNonObligatedWeeeForReturnRequestHandler : IRequestHandler<Request, List<decimal?>>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly IFetchNonObligatedWeeeForReturnDataAccess dataAccess;

        public FetchNonObligatedWeeeForReturnRequestHandler(
            IFetchNonObligatedWeeeForReturnDataAccess dataAccess, IWeeeAuthorization authdataaccess)
        {
            this.dataAccess = dataAccess;
            this.authorization = authdataaccess;
        }

        public async Task<List<decimal?>> HandleAsync(Request message)
        {
            authorization.EnsureCanAccessExternalArea();
            authorization.EnsureOrganisationAccess(message.OrganisationsId);

            return await dataAccess.FetchNonObligatedWeeeForReturn(message.ReturnId, message.Dcf);
        }
    }
}