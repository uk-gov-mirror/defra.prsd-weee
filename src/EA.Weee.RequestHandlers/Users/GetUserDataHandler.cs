﻿namespace EA.Weee.RequestHandlers.Users
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Users;
    using DataAccess;
    using Prsd.Core.Domain;
    using Prsd.Core.Mediator;
    using Requests.Users;

    internal class GetUserDataHandler : IRequestHandler<GetUserData, UserData>
    {
        private readonly WeeeContext context;
        private readonly IUserContext userContext;

        public GetUserDataHandler(WeeeContext context, IUserContext userContext)
        {
            this.context = context;
            this.userContext = userContext;
        }

        public async Task<UserData> HandleAsync(GetUserData query)
        {
            string userId = userContext.UserId.ToString();
            if (query.Id != userId)
            {
                throw new UnauthorizedAccessException(
                    string.Format("User {0} is not able to access data relating to another user ({1})", userId, query.Id));
            }

            return await context.Users.Select(u => new UserData
            {
                Email = u.Email,
                FirstName = u.FirstName,
                Id = u.Id,
                Surname = u.Surname,
            }).SingleOrDefaultAsync(u => u.Id == query.Id);
        }
    }
}