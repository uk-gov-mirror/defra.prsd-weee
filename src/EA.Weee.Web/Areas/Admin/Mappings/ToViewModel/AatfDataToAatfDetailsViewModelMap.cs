﻿namespace EA.Weee.Web.Areas.Admin.Mappings.ToViewModel
{
    using EA.Prsd.Core;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.Admin.ViewModels.Aatf;
    using EA.Weee.Web.ViewModels.Shared.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AatfDataToAatfDetailsViewModelMap : IMap<AatfDataToAatfDetailsViewModelMapTransfer, AatfDetailsViewModel>
    {
        private readonly IAddressUtilities addressUtilities;

        public AatfDataToAatfDetailsViewModelMap(IAddressUtilities addressUtilities)
        {
            this.addressUtilities = addressUtilities;
        }

        public AatfDetailsViewModel Map(AatfDataToAatfDetailsViewModelMapTransfer source)
        {
            Guard.ArgumentNotNull(() => source, source);
            Guard.ArgumentNotNull(() => source.AatfData, source.AatfData);

            AatfDetailsViewModel viewModel = new AatfDetailsViewModel()
            {
                Id = source.AatfData.Id,
                Name = source.AatfData.Name,
                ApprovalNumber = source.AatfData.ApprovalNumber,
                CompetentAuthority = source.AatfData.CompetentAuthority,
                AatfStatus = source.AatfData.AatfStatus,
                SiteAddress = source.AatfData.SiteAddress,
                Size = source.AatfData.Size,
                ContactData = source.AatfData.Contact,
                CanEdit = source.AatfData.Contact.CanEditContactDetails,
                Organisation = source.AatfData.Organisation,
                OrganisationAddress = source.OrganisationString,
                FacilityType = source.AatfData.FacilityType,
                ComplianceYear = source.AatfData.ComplianceYear,
                SiteAddressLong = addressUtilities.FormattedAddress(source.AatfData.SiteAddress, false),
                ContactAddressLong = addressUtilities.FormattedAddress(source.AatfData.Contact.AddressData, false)
            };

            if (source.AssociatedAatfs != null)
            {
                var associatedAEs = source.AssociatedAatfs.Where(a => a.FacilityType == FacilityType.Ae && a.Id != source.AatfData.Id).ToList();
                source.AssociatedAatfs = source.AssociatedAatfs.Where(a => a.Id != source.AatfData.Id && a.FacilityType == FacilityType.Aatf).ToList();
                viewModel.AssociatedAatfs = source.AssociatedAatfs;
                viewModel.AssociatedAes = associatedAEs;
            }

            if (source.AssociatedSchemes != null)
            {
                viewModel.AssociatedSchemes = source.AssociatedSchemes;
            }

            if (source.AatfData.ApprovalDate != default(DateTime))
            {
                viewModel.ApprovalDate = source.AatfData.ApprovalDate.GetValueOrDefault();
            }

            return viewModel;
        }
    }
}