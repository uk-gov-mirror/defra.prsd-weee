﻿namespace EA.Weee.RequestHandlers.Mappings
{
    using Core.NewUser;
    using Domain;
    using Prsd.Core.Mapper;

    public class UserMap : IMap<User, UserData>
    {
        public UserData Map(User source)
        {
            return new UserData
            {
                Id = source.Id,
                Email = source.Email,
                FirstName = source.FirstName,
                Surname = source.Surname
            };
        }
    }
}