﻿namespace EA.Weee.RequestHandlers.Mappings
{
    using Core.Shared;
    using Domain.Organisation;
    using Prsd.Core.Mapper;

    public class AddressMap : IMap<Address, AddressData>
    {
        public AddressData Map(Address source)
        {
            return new AddressData
            {
                Address1 = source.Address1,
                Address2 = source.Address2,
                TownOrCity = source.TownOrCity,
                CountyOrRegion = source.CountyOrRegion,
                Postcode = source.Postcode,
                CountryId = source.Country.Id,
                CountryName = source.Country.Name,
                Telephone = source.Telephone,
                Email = source.Email
            };
        }
    }
}
