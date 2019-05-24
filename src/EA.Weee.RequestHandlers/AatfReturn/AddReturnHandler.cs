﻿namespace EA.Weee.RequestHandlers.AatfReturn
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using DataAccess.DataAccess;
    using Domain.AatfReturn;
    using Domain.DataReturns;
    using Domain.Organisation;
    using Domain.User;
    using NonObligated;
    using Organisations;
    using Prsd.Core.Domain;
    using Prsd.Core.Mediator;
    using Requests.AatfReturn;
    using Requests.AatfReturn.NonObligated;
    using Security;
    using Specification;

    internal class AddReturnHandler : IRequestHandler<AddReturn, Guid>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly IReturnDataAccess returnDataAccess;
        private readonly IOrganisationDataAccess organisationDataAccess;
        private readonly IGenericDataAccess genericDataAccess;
        private readonly IUserContext userContext;

        public AddReturnHandler(IWeeeAuthorization authorization, 
            IReturnDataAccess returnDataAccess, 
            IOrganisationDataAccess organisationDataAccess, 
            IGenericDataAccess genericDataAccess, 
            IUserContext userContext)
        {
            this.authorization = authorization;
            this.returnDataAccess = returnDataAccess;
            this.organisationDataAccess = organisationDataAccess;
            this.genericDataAccess = genericDataAccess;
            this.userContext = userContext;
        }

        public async Task<Guid> HandleAsync(AddReturn message)
        {
            authorization.EnsureCanAccessExternalArea();
            authorization.EnsureOrganisationAccess(message.OrganisationId);

            var quarter = new Quarter(2019, QuarterType.Q1);

            var aatfOperator = await genericDataAccess.GetSingleByExpression<Operator>(new OperatorByOrganisationIdSpecification(message.OrganisationId));

            if (aatfOperator == null)
            {
                var organisation = await genericDataAccess.GetById<Organisation>(message.OrganisationId);

                aatfOperator = new Operator(organisation);

                await genericDataAccess.Add<Operator>(aatfOperator);
            }

            var aatfReturn = new Return(aatfOperator, quarter, userContext.UserId.ToString());

            await returnDataAccess.Submit(aatfReturn);

            return aatfReturn.Id;
        }
    }
}
