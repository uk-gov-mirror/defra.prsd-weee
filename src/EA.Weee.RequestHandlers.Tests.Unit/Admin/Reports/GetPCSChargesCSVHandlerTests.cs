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

    public class GetPCSChargesCSVHandlerTests
    {
        [Fact]
        public async Task GetPCSChargesCSVHandler_NotInternalUser_ThrowsSecurityException()
        {
            // Arrange
            var complianceYear = 2016;

            var authorization = new AuthorizationBuilder().DenyInternalAreaAccess().Build();
            var context = A.Fake<WeeeContext>();
            var csvWriterFactory = A.Fake<CsvWriterFactory>();

            var handler = new GetPCSChargesCSVHandler(authorization, context, csvWriterFactory);
            var request = new GetPCSChargesCSV(complianceYear);

            // Act
            Func<Task> action = async () => await handler.HandleAsync(request);

            // Assert
            await Assert.ThrowsAsync<SecurityException>(action);
        }

        [Fact]
        public async Task GetPCSChargesCSVHandler_NoComplianceYear_ThrowsArgumentException()
        {
            // Arrange
            var complianceYear = 0;

            var authorization = new AuthorizationBuilder().AllowInternalAreaAccess().Build();
            var context = A.Fake<WeeeContext>();
            var csvWriterFactory = A.Fake<CsvWriterFactory>();

            var handler = new GetPCSChargesCSVHandler(authorization, context, csvWriterFactory);
            var request = new GetPCSChargesCSV(complianceYear);

            // Act
            Func<Task> action = async () => await handler.HandleAsync(request);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(action);
        }
        
        [Fact]
        public async Task GetPCSChargesCSVHandler_ComplianceYear_ReturnsFileContent()
        {
            // Arrange
            var complianceYear = 2016;

            var authorization = new AuthorizationBuilder().AllowInternalAreaAccess().Build();
            var context = A.Fake<WeeeContext>();
            var csvWriterFactory = A.Fake<CsvWriterFactory>();

            var handler = new GetPCSChargesCSVHandler(authorization, context, csvWriterFactory);
            var request = new GetPCSChargesCSV(complianceYear);

            // Act
            CSVFileData data = await handler.HandleAsync(request);

            // Assert
            Assert.NotEmpty(data.FileContent);
        }
    }
}