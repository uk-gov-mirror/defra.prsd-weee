﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA.Weee.Core.PCS.MemberUploadTesting
{
    public class SchemeBusiness
    {
        public SchemeCompany Company { get; set; }
        public SchemePartnership Partnership { get; set; }

        public SchemeBusiness()
        {

        }

        public static SchemeBusiness Create(ISchemeBusinessSettings settings)
        {
            SchemeBusiness schemeBusiness = new SchemeBusiness();

            if (RandomHelper.OneIn(2))
            {
                schemeBusiness.Company = SchemeCompany.Create(settings);
            }
            else
            {
                schemeBusiness.Partnership = SchemePartnership.Create(settings);
            }

            return schemeBusiness;
        }
    }
}
