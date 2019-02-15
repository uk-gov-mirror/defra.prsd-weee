﻿namespace EA.Weee.RequestHandlers.AatfReturn.NonObligated
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.AatfReturn;
    using DataAccess;
    using Domain.AatfReturn;
    using Domain.DataReturns;
    using Domain.Organisation;
    using Prsd.Core.Domain;
    using Prsd.Core.Mediator;
    using Requests.AatfReturn;
    using Requests.AatfReturn.NonObligated;
    using Security;

    internal class AddNonObligatedHandler : IRequestHandler<AddNonObligated, bool>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly IAddNonObligatedDataAccess nonObligatedDataAccess;
        private readonly IReturnDataAccess returnDataAccess;

        public AddNonObligatedHandler(IWeeeAuthorization authorization, 
            IAddNonObligatedDataAccess nonObligatedDataAccess, 
            IReturnDataAccess returnDataAccess)
        {
            this.authorization = authorization;
            this.nonObligatedDataAccess = nonObligatedDataAccess;
            this.returnDataAccess = returnDataAccess;
        }

        public async Task<bool> HandleAsync(AddNonObligated message)
        {
            authorization.EnsureCanAccessExternalArea();

            var aatfReturn = await returnDataAccess.GetById(message.ReturnId);

            var nonObligatedWee = new List<NonObligatedWeee>();

            foreach (var categoryValue in message.CategoryValues)
            {
                nonObligatedWee.Add(new NonObligatedWeee(aatfReturn, categoryValue.CategoryId, message.Dcf, categoryValue.Tonnage));
            }

            await nonObligatedDataAccess.Submit(nonObligatedWee);

            return true;
        }
    }
}