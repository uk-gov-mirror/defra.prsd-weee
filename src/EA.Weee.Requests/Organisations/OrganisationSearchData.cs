﻿namespace EA.Weee.Requests.Organisations
{
    using System;
    using Shared;

    public class OrganisationSearchData
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public AddressData Address { get; set; }
    }
}
