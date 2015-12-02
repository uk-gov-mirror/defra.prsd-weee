﻿namespace EA.Weee.RequestHandlers.DataReturns.GenerateDomainObjects
{
    using System;
    using System.Collections.Generic;
    using Domain.Scheme;
    using Requests.DataReturns;

    public interface IGenerateFromDataReturnsXML
    { 
        DataReturnsUpload GenerateDataReturnsUpload(ProcessDataReturnsXMLFile messageXmlFile, List<DataReturnsUploadError> errors, Guid schemeId);
    }
}
