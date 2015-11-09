﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Admin.Reports
{
    using System;
    using System.Security;
    using System.Threading.Tasks;
    using Core.Admin;
    using Core.Shared;
    using DataAccess;
    using FakeItEasy;
    using RequestHandlers.Admin.Reports;
    using Requests.Admin;
    using Weee.Tests.Core;
    using Xunit;

    public class GetMembersDetailsCSVHandlerTests
    {
        [Fact]
        public async Task GetMembersDetailsCSVHandler_NoComplianceYear_ThrowsSecurityException()
        {
            // Arrange
            var complianceYear = 0;

            var authorization = new AuthorizationBuilder().DenyOrganisationAccess().Build();
            var context = A.Fake<WeeeContext>();
            var csvWriterFactory = A.Fake<CsvWriterFactory>();

            var handler = new GetMembersDetailsCSVHandler(authorization, context, csvWriterFactory);
            var request = new GetMemberDetailsCSV(complianceYear);

            // Act
            Func<Task> action = async () => await handler.HandleAsync(request);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(action);
        }

        [Fact]
        public async Task GetMembersDetailsCSVHandler_ComplianceYear_ReturnsFileContent()
        {
            // Arrange
            var complianceYear = 2016;

            var authorization = new AuthorizationBuilder().DenyOrganisationAccess().Build();
            var context = A.Fake<WeeeContext>();
            var csvWriterFactory = A.Fake<CsvWriterFactory>();

            var handler = new GetMembersDetailsCSVHandler(authorization, context, csvWriterFactory);
            var request = new GetMemberDetailsCSV(complianceYear);

            // Act
            MembersDetailsCSVFileData data = await handler.HandleAsync(request);

            // Assert
            Assert.NotEmpty(data.FileContent);
        }
    }
}