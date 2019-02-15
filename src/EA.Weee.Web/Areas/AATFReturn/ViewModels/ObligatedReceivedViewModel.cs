﻿namespace EA.Weee.Web.Areas.AatfReturn.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Core.Validation;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Helpers;

    public class ObligatedReceivedViewModel
    {
        public Guid OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public Guid PcsId { get; set; }

        public string PcsName { get; set; }

        public Guid ReturnId { get; set; }

        public IList<ObligatedCategoryValue> CategoryValues { get; set; }

        public ObligatedReceivedViewModel()
        {
            AddCategoryValues(new ObligatedCategoryValues());
        }

        public ObligatedReceivedViewModel(ObligatedCategoryValues values)
        {
            AddCategoryValues(values);
        }

        private void AddCategoryValues(ObligatedCategoryValues obligatedCategories)
        {
            CategoryValues = new List<ObligatedCategoryValue>();

            foreach (var categoryValue in obligatedCategories)
            {
                CategoryValues.Add(categoryValue);
            }
        }
        public string B2CTotal => Total(CategoryValues, true);

        public string B2BTotal => Total(CategoryValues, false);

        public string Total(IList<ObligatedCategoryValue> categoryValues, bool isHousehold)
        {
            var total = 0.000m;
            List<string> values;

            if (isHousehold)
            {
                values = categoryValues.Where(c => !string.IsNullOrWhiteSpace(c.B2C)
                                                    && decimal.TryParse(c.B2C, NumberStyles.Number & ~NumberStyles.AllowLeadingSign & ~NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out var output)
                                                    && output.DecimalPlaces() <= 3).Select(c => c.B2C).ToList();
            }
            else
            {
                values = categoryValues.Where(c => !string.IsNullOrWhiteSpace(c.B2B) 
                                                    && decimal.TryParse(c.B2B, NumberStyles.Number & ~NumberStyles.AllowLeadingSign & ~NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out var output)
                                                    && output.DecimalPlaces() <= 3).Select(c => c.B2B).ToList();
            }

            if (values.Any())
            {
                var convertedValues = values.ConvertAll(Convert.ToDecimal);
                total = convertedValues.Sum();
            }

            return total.ToTonnageDisplay();
        }
    }
}