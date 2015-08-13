﻿namespace EA.Weee.Domain.Organisation
{
    using System;
    using EA.Prsd.Core.Domain;

    public class OrganisationUser : Entity
    {
        public string UserId { get; private set; }

        public Guid OrganisationId { get; private set; }

        public UserStatus UserStatus { get; private set; }

        public virtual Organisation Organisation { get; private set; }

        protected OrganisationUser()
        {
        }

        public OrganisationUser(Guid userId, Guid organisationId, UserStatus userStatus)
        {
            UserId = userId.ToString();
            OrganisationId = organisationId;
            UserStatus = userStatus;
        }
    }
}