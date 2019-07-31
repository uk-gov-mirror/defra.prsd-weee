﻿namespace EA.Weee.RequestHandlers.Admin.AatfReports
{
    using System;
    using System.Threading.Tasks;
    using Core.Admin;
    using Core.Shared;
    using DataAccess;
    using DataAccess.StoredProcedure;
    using Prsd.Core.Mediator;
    using Requests.Admin.AatfReports;
    using Security;

    internal class GetAatfAeReturnDataCsvHandler : IRequestHandler<GetAatfAeReturnDataCsv, CSVFileData>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly WeeeContext context;
        private readonly CsvWriterFactory csvWriterFactory;

        public GetAatfAeReturnDataCsvHandler(IWeeeAuthorization authorization, WeeeContext context,
          CsvWriterFactory csvWriterFactory)
        {
            this.authorization = authorization;
            this.context = context;
            this.csvWriterFactory = csvWriterFactory;
        }
        public async Task<CSVFileData> HandleAsync(GetAatfAeReturnDataCsv request)
        {
            authorization.EnsureCanAccessInternalArea();

            if (request.ComplianceYear == 0)
            {
                var message = $"Compliance year cannot be \"{request.ComplianceYear}\".";
                throw new ArgumentException(message);
            }

            var items = await context.StoredProcedures.GetAatfAeReturnDataCsvData(
                       request.ComplianceYear, request.Quarter, (int)request.FacilityType, 
                       request.ReturnStatus, request.AuthorityId, request.LocalArea, request.PanArea);

            foreach (var item in items)
            {
                 item.AatfDataUrl = $@" =HYPERLINK(""""{request.AatfDataUrl}{item.AatfId}#data"""", """"View AATF / AE data"""")";
            }

            var csvWriter = csvWriterFactory.Create<AatfAeReturnData>();
            csvWriter.DefineColumn(@"Name of AATF / AE", i => i.Name);
            csvWriter.DefineColumn(@"Approval number", i => i.ApprovalNumber);
            csvWriter.DefineColumn(@"Organisation name", i => i.OrganisationName);
            csvWriter.DefineColumn(@"Submission status", i => i.ReturnStatus);
            csvWriter.DefineColumn(@"Date created (GMT)", i => i.CreatedDate);
            csvWriter.DefineColumn(@"Date submitted (GMT)", i => i.SubmittedDate);
            csvWriter.DefineColumn(@"Submitted by", i => i.SubmittedBy);
            csvWriter.DefineColumn(@"Appropriate authority", i => i.CompetentAuthorityAbbr);
            csvWriter.DefineColumn(@" ", i => i.AatfDataUrl);
            var fileContent = csvWriter.Write(items);

            //Trim the space before equals in  =Hyperlink
            fileContent = fileContent.Replace(" =HYPERLINK", "=HYPERLINK");

            var fileName = string.Format("{0}_{2}_RETURN_{1:ddMMyyyy_HHmm}_Q{3}.csv",
                request.ComplianceYear,
                DateTime.UtcNow,
                request.FacilityType.ToString().ToUpper(),
                request.Quarter);

            return new CSVFileData
            {
                FileContent = fileContent,
                FileName = fileName
            };
        }
    }
}