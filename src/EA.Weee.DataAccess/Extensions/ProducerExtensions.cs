﻿namespace EA.Weee.DataAccess.Extensions
{
    using System.Data.Entity;
    using System.Linq;
    using Domain.Producer;

    public static class ProducerExtensions
    {
        public static IQueryable<string> ProducerNames(this DbSet<Producer> producers)
        {
            return producers.Select(p => p.ProducerBusiness != null
                ? p.ProducerBusiness.CompanyDetails != null
                    ? p.ProducerBusiness.CompanyDetails.Name
                    : p.ProducerBusiness.Partnership != null
                        ? p.ProducerBusiness.Partnership.Name
                        : p.TradingName
                : p.TradingName);
        }
    }
}