﻿namespace EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.AatfReturn;
    using EA.Weee.Core.Scheme;
    using Prsd.Core;
    using Prsd.Core.Mapper;
    using ViewModels;

    public class ReturnToReturnViewModelMap : IMap<ReturnData, ReturnViewModel>
    {
        public decimal? NonObligatedTonnageTotal = null;
        public decimal? NonObligatedTonnageTotalDcf = null;

        public List<AatfObligatedData> AatfObligatedData = new List<AatfObligatedData>();
        private readonly ITonnageUtilities tonnageUtilities;

        public ReturnToReturnViewModelMap(ITonnageUtilities tonnageUtilities)
        {
            this.tonnageUtilities = tonnageUtilities;
        }

        public ReturnViewModel Map(ReturnData source)
        {
            Guard.ArgumentNotNull(() => source, source);

            if (source.NonObligatedData != null)
            {
                foreach (var category in source.NonObligatedData)
                {
                    if (category.Dcf && category.Tonnage != null)
                    {
                        NonObligatedTonnageTotalDcf = tonnageUtilities.InitialiseTotalDecimal(NonObligatedTonnageTotalDcf);
                        NonObligatedTonnageTotalDcf += category.Tonnage;
                    }
                    else if (!category.Dcf && category.Tonnage != null)
                    {
                        NonObligatedTonnageTotal = tonnageUtilities.InitialiseTotalDecimal(NonObligatedTonnageTotal);
                        NonObligatedTonnageTotal += category.Tonnage;
                    }
                }
            }

            if (source.Aatfs != null)
            {
                foreach (var aatf in source.Aatfs)
                {
                    var weeeReceivedData = source.ObligatedWeeeReceivedData.Where(s => s.Aatf.Id == aatf.Id).ToList();
                    var weeeReusedData = source.ObligatedWeeeReusedData.Where(s => s.Aatf.Id == aatf.Id).ToList();

                    var schemeData = new List<AatfSchemeData>(); 

                    foreach (var scheme in source.SchemeDataItems)
                    {
                        var schemeList = weeeReceivedData.Where(s => s.Scheme.Id == scheme.Id).ToList();

                        foreach (var item in schemeList)
                        {
                            ObligatedCategoryValue obligatedReceivedValues = new ObligatedCategoryValue
                            {
                                B2B = tonnageUtilities.CheckIfTonnageIsNull(item.B2B),
                                B2C = tonnageUtilities.CheckIfTonnageIsNull(item.B2C)
                            };

                            var aatfSchemeData = new AatfSchemeData(item.Scheme, obligatedReceivedValues, scheme.ApprovalName);
                            schemeData.Add(aatfSchemeData);
                        }                       
                    }                    

                    var obligatedData = new AatfObligatedData(aatf, schemeData)
                    {
                        WeeeReceived = tonnageUtilities.SumObligatedValues(weeeReceivedData),
                        WeeeReused = tonnageUtilities.SumObligatedValues(weeeReusedData)
                    };

                    AatfObligatedData.Add(obligatedData);
                }
            }

            return new ReturnViewModel(
                source.Quarter,
                source.QuarterWindow,
                source.Quarter.Year,
                tonnageUtilities.CheckIfTonnageIsNull(NonObligatedTonnageTotal),
                tonnageUtilities.CheckIfTonnageIsNull(NonObligatedTonnageTotalDcf),
                AatfObligatedData,
                source.ReturnOperatorData,
                source.Id);
        }
    }
}