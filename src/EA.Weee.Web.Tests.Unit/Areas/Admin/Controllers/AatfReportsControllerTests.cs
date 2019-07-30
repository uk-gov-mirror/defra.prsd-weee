﻿namespace EA.Weee.Web.Tests.Unit.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Api.Client;
    using AutoFixture;
    using Core.AatfReturn;
    using Core.Admin;
    using Core.Shared;
    using EA.Weee.Requests.Admin.AatfReports;
    using EA.Weee.Web.Areas.Admin.ViewModels.AatfReports;
    using FakeItEasy;
    using FluentAssertions;
    using Services;
    using TestHelpers;
    using Web.Areas.Admin.Controllers;
    using Web.Areas.Admin.Controllers.Base;
    using Web.Areas.Admin.ViewModels.Reports;
    using Web.Infrastructure;
    using Weee.Requests.Admin;
    using Weee.Requests.Admin.GetActiveComplianceYears;
    using Weee.Requests.Shared;
    using Xunit;

    public class AatfReportsControllerTests
    {
        private readonly Fixture fixture;
        private readonly AatfReportsController controller;
        private readonly IWeeeClient weeeClient;
        private readonly BreadcrumbService breadcrumb;

        public AatfReportsControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            breadcrumb = A.Fake<BreadcrumbService>();

            controller = new AatfReportsController(() => weeeClient, breadcrumb);

            fixture = new Fixture();
        }

        [Fact]
        public void AatfReportsController_ShouldInheritFromAdminController()
        {
            typeof(AatfReportsController).Should().BeDerivedFrom<AdminController>();
        }

        [Fact]
        public async Task GetAatfAeReturnData_Always_ReturnsAatfAeReturnDataViewModel()
        {
            var competentAuthorities = fixture.CreateMany<UKCompetentAuthorityData>().ToList();
            var panAreas = fixture.CreateMany<PanAreaData>().ToList();
            var localAreas = fixture.CreateMany<LocalAreaData>().ToList();
            var years = fixture.CreateMany<int>().ToList();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).Returns(years);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).Returns(panAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetLocalAreas>._)).Returns(localAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).Returns(competentAuthorities);

            // Act
            var result = await controller.AatfAeReturnData();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "AatfAeReturnData");

            var model = viewResult.Model as AatfAeReturnDataViewModel;
            Assert.NotNull(model);

            model.ComplianceYears.Select(c => c.Text).Should().BeEquivalentTo(years.Select(y => y.ToString()));
            model.PanAreaList.Select(c => c.Text).Should().BeEquivalentTo(panAreas.Select(y => y.Name.ToString()));
            model.PanAreaList.Select(c => c.Value).Should().BeEquivalentTo(panAreas.Select(y => y.Id.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Text).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Abbreviation.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Value).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Id.ToString()));

            Assert.Collection(model.Quarters,
                s1 => Assert.Equal("1", s1.Text),
                s2 => Assert.Equal("2", s2.Text),
                s3 => Assert.Equal("3", s3.Text),
                s4 => Assert.Equal("4", s4.Text));

            Assert.Collection(model.FacilityTypes,
               s1 => Assert.Equal("AATF", s1.Text),
               s2 => Assert.Equal("AE", s2.Text));

            Assert.Collection(model.SubmissionStatus,
               s1 => Assert.Equal("Submitted", s1.Text),
               s2 => Assert.Equal("Started", s2.Text),
               s3 => Assert.Equal("Not Started", s3.Text));

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetLocalAreas>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        /// <summary>
        /// This test ensures that the GET "AatfAeReturnData" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAatfAeReturnData_Always_SetsTriggerDownloadToFalse()
        {
            // Act
            var result = await controller.AatfAeReturnData();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "AatfAeReturnData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAatfAeReturnData_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.AatfAeReturnData();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "AatfAeReturnData" action with an invalid view model
        /// calls the API to retrieve the list of compliance years and returns the "AatfAeReturnData"
        /// view with a AatfAeReturnDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfAeReturnData_WithInvalidViewModel_ReturnsAatfAeReturnDataViewModel()
        {
            var competentAuthorities = fixture.CreateMany<UKCompetentAuthorityData>().ToList();
            var panAreas = fixture.CreateMany<PanAreaData>().ToList();
            var localAreas = fixture.CreateMany<LocalAreaData>().ToList();
            var years = fixture.CreateMany<int>().ToList();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).Returns(years);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).Returns(panAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetLocalAreas>._)).Returns(localAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).Returns(competentAuthorities);

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfAeReturnData(new AatfAeReturnDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "AatfAeReturnData");

            var model = viewResult.Model as AatfAeReturnDataViewModel;
            Assert.NotNull(model);

            model.ComplianceYears.Select(c => c.Text).Should().BeEquivalentTo(years.Select(y => y.ToString()));
            model.PanAreaList.Select(c => c.Text).Should().BeEquivalentTo(panAreas.Select(y => y.Name.ToString()));
            model.PanAreaList.Select(c => c.Value).Should().BeEquivalentTo(panAreas.Select(y => y.Id.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Text).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Abbreviation.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Value).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Id.ToString()));
        }

        /// <summary>
        /// This test ensures that the POST "AatfAeReturnData" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfAeReturnData_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfAeReturnData(new AatfAeReturnDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "AatfAeReturnData" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfAeReturnData_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Act
            var result = await controller.AatfAeReturnData(new AatfAeReturnDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "AatfAeReturnData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfAeReturnData_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.AatfAeReturnData(A.Dummy<AatfAeReturnDataViewModel>());

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        [Fact]
        public async void PostAatfAeReturnData_OnDownload_SetsURL()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);
            var httpRequest = A.Fake<HttpRequestBase>();

            var file = new CSVFileData() { FileContent = "Content", FileName = "test.csv" };

            var uri = new Uri("https://localhost:44300");
            A.CallTo(() => controller.HttpContext.Request).Returns(httpRequest);
            A.CallTo(() => controller.HttpContext.Request.Url).Returns(uri);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfAeReturnDataCsv>._)).Returns(file);

            var result = await controller.DownloadAatfAeDataCsv(2019, 1,
                FacilityType.Aatf, A.Dummy<int?>(), A.Dummy<Guid?>(), A.Dummy<Guid?>(),
                A.Dummy<Guid?>());

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfAeReturnDataCsv>.That.Matches(x => x.AatfDataUrl.Equals("https://localhost:44300/admin/aatf/details/")))).MustHaveHappened();

            var fileResult = result as FileResult;
            Assert.NotNull(fileResult);
            Assert.Equal("text/csv", fileResult.ContentType);
        }

        [Fact]
        public async void PostAatfAeReturnData_OnDownload_SetsURLWithVirtualDirectory()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);
            var httpRequest = A.Fake<HttpRequestBase>();

            var file = new CSVFileData() { FileContent = "Content", FileName = "test.csv" };

            var uri = new Uri("https://localhost:44300/weeerelease");
            A.CallTo(() => controller.HttpContext.Request).Returns(httpRequest);
            A.CallTo(() => controller.HttpContext.Request.Url).Returns(uri);
            A.CallTo(() => controller.HttpContext.Request.ApplicationPath).Returns("weeerelease");

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfAeReturnDataCsv>._)).Returns(file);

            var result = await controller.DownloadAatfAeDataCsv(2019, 1,
                FacilityType.Aatf, A.Dummy<int?>(), A.Dummy<Guid?>(), A.Dummy<Guid?>(),
                A.Dummy<Guid?>());

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfAeReturnDataCsv>.That.Matches(x => x.AatfDataUrl.Equals("https://localhost:44300/weeerelease/admin/aatf/details/")))).MustHaveHappened();

            var fileResult = result as FileResult;
            Assert.NotNull(fileResult);
            Assert.Equal("text/csv", fileResult.ContentType);
        }

        /// <summary>
        /// This test ensures that the GET "UKWeeeDataAtAatfs" action calls the API to retrieve
        /// the list of compliance years and returns the "UKWeeeDataAtAatfs" view with 
        /// a ProducerDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUkWeeeDataAtAatfs_Always_ReturnsUKWeeeDataAtAatfsProducerDataViewModel()
        {
            // Arrange
            var years = new List<int>() { 2001, 2002 };

            A.CallTo(() => weeeClient.SendAsync(A<string>.Ignored, A<GetAatfReturnsActiveComplianceYears>.Ignored)).Returns(years);

            // Act
            var result = await controller.UkWeeeDataAtAatfs();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName));

            var model = viewResult.Model as UkWeeeDataAtAatfViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));
        }

        /// <summary>
        /// This test ensures that the GET "UKWeeeDataAtAatfs" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUkWeeeDataAtAatfs_Always_SetsTriggerDownloadToFalse()
        {
            // Act
            var result = await controller.UkWeeeDataAtAatfs();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "UKWeeeDataAtAatfs" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUkWeeeDataAtAatfs_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.UkWeeeDataAtAatfs();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "UkWeeeDataAtAatfs" action with an invalid view model
        /// calls the API to retrieve the list of compliance years and returns the "UkWeeeDataAtAatfs"
        /// view with a ProducerDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUkWeeeDataAtAatfs_WithInvalidViewModel_ReturnsUkWeeeDataAtAatfsProducerDataViewModel()
        {
            // Arrange
            var years = new List<int>() { 2001, 2002 };

            A.CallTo(() => weeeClient.SendAsync(A<string>.Ignored, A<GetAatfReturnsActiveComplianceYears>.Ignored)).Returns(years);

            var viewModel = new UkWeeeDataAtAatfViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.UkWeeeDataAtAatfs(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName));

            var model = viewResult.Model as UkWeeeDataAtAatfViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));
        }

        /// <summary>
        /// This test ensures that the POST "UkWeeeDataAtAatfs" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUkWeeeDataAtAatfs_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Arrange
            var viewModel = new UkWeeeDataAtAatfViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.UkWeeeDataAtAatfs(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "UkWeeeDataAtAatfs" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUkWeeeDataAtAatfs_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Arrange
           var viewModel = new UkWeeeDataAtAatfViewModel();

            // Act
            var result = await controller.UkWeeeDataAtAatfs(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "UkWeeeDataAtAatfs" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUkWeeeDataAtAatfs_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.UkWeeeDataAtAatfs(A.Dummy<UkWeeeDataAtAatfViewModel>());

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadUkWeeeDataAtAatfsCsv" action will
        /// call the API to generate a CSV file which is returned with the correct file name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadUkWeeeDataAtAatfsCsv_Always_CallsApiAndReturnsFileResultWithCorrectFileName()
        {
            // Arrange
            var filename = fixture.Create<string>();
            var file = new FileInfo(filename, new byte[] { 1, 2, 3 });

            A.CallTo(() => weeeClient.SendAsync(A<string>.Ignored, A<GetUkWeeeAtAatfsCsv>.Ignored))
                .WhenArgumentsMatch(a => a.Get<GetUkWeeeAtAatfsCsv>("request").ComplianceYear == 2015)
                .Returns(file);

            var viewModel = new ProducersDataViewModel();

            // Act
            var result = await controller.DownloadUkWeeeDataAtAatfsCsv(2015);

            // Assert
            var fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal(filename, fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadUkWeeeDataAtAatfsCsv" action will
        /// call the API to generate a CSV file which is returned with a content
        /// type of "text/csv"
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadUkWeeeDataAtAatfsCsv_Always_CallsApiAndReturnsFileResultWithContentTypeOfTextCsv()
        {
            // Arrange
            A.CallTo(() => weeeClient.SendAsync(A<GetUkWeeeCsv>.Ignored))
                .WhenArgumentsMatch(a => a.Get<int>("complianceYear") == 2015)
                .Returns(A.Dummy<FileInfo>());

            var viewModel = new ProducersDataViewModel();

            // Act
            var result = await controller.DownloadUkWeeeDataAtAatfsCsv(2015);

            // Assert
            var fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("text/csv", fileResult.ContentType);
        }

        [Fact]
        public async void Get_UkNonObligatedWeeeReceived_ShouldReturnsUkNonObligatedWeeeReceivedView()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._))
                .Returns(new List<int> { 2015, 2016 });

            var result = await controller.UkNonObligatedWeeeReceived();

            var viewResult = ((ViewResult)result);
            Assert.Equal("UkNonObligatedWeeeReceived", viewResult.ViewName);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task GetUkNonObligatedWeeeReceived_Always_SetsTriggerDownloadToFalse()
        {
            var result = await controller.UkNonObligatedWeeeReceived();

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        [Fact]
        public async void Post_UkNonObligatedWeeeReceived_ModelIsInvalid_ShouldRedirectViewWithError()
        {
            controller.ModelState.AddModelError("Key", "Any error");

            var result = await controller.UkNonObligatedWeeeReceived(new UkNonObligatedWeeeReceivedViewModel());

            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task PostUkNonObligatedWeeeReceived_ModelIsInvalid_SetsTriggerDownloadToFalse()
        {
            var viewModel = new UkNonObligatedWeeeReceivedViewModel();

            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.UkNonObligatedWeeeReceived(viewModel);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        [Fact]
        public async Task PostUkNonObligatedWeeeReceived_ValidModel_SetsTriggerDownloadToTrue()
        {
            var viewModel = new UkNonObligatedWeeeReceivedViewModel();

            var result = await controller.UkNonObligatedWeeeReceived(viewModel);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        [Fact]
        public async Task GetDownloadUkNonObligatedWeeeReceivedCsv_ReturnsFileResultWithContentTypeOfTextCsv()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUkNonObligatedWeeeReceivedDataCsv>._)).Returns(new CSVFileData
            {
                FileContent = "UK non-obligated WEEE REPORT",
                FileName = "test.csv"
            });

            var result = await controller.DownloadUkNonObligatedWeeeReceivedCsv(2015);

            var fileResult = result as FileResult;
            Assert.NotNull(fileResult);
            Assert.Equal("text/csv", fileResult.ContentType);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUkNonObligatedWeeeReceivedDataCsv>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task GetAatfObligatedData_Always_ReturnsAatfObligatedDataViewModel()
        {
            var competentAuthorities = fixture.CreateMany<UKCompetentAuthorityData>().ToList();
            var panAreas = fixture.CreateMany<PanAreaData>().ToList();
            var localAreas = fixture.CreateMany<LocalAreaData>().ToList();
            var years = fixture.CreateMany<int>().ToList();
            
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).Returns(years);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).Returns(panAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetLocalAreas>._)).Returns(localAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).Returns(competentAuthorities);

            // Act
            var result = await controller.AatfObligatedData();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "AatfObligatedData");

            var model = viewResult.Model as AatfObligatedDataViewModel;
            Assert.NotNull(model);

            model.ComplianceYears.Select(c => c.Text).Should().BeEquivalentTo(years.Select(y => y.ToString()));
            model.PanAreaList.Select(c => c.Text).Should().BeEquivalentTo(panAreas.Select(y => y.Name.ToString()));
            model.PanAreaList.Select(c => c.Value).Should().BeEquivalentTo(panAreas.Select(y => y.Id.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Text).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Abbreviation.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Value).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Id.ToString()));

            Assert.Collection(model.ObligationTypes,
                s1 => Assert.Equal("B2B", s1.Text),
                s2 => Assert.Equal("B2C", s2.Text));

            Assert.Collection(model.SchemeColumnPossibleValues,
               s1 => Assert.Equal("PCS names", s1.Text),
               s2 => Assert.Equal("Approval numbers", s2.Text));

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        /// <summary>
        /// This test ensures that the GET "AatfObligatedData" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAatfObligatedData_Always_SetsTriggerDownloadToFalse()
        {
            // Act
            var result = await controller.AatfObligatedData();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "AatfObligatedData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAatfObligatedData_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.AatfObligatedData();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "AatfObligatedData" action with an invalid view model
        /// calls the API to retrieve the list of compliance years and returns the "AatfObligatedData"
        /// view with a AatfObligatedDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfObligatedData_WithInvalidViewModel_ReturnsAatfObligatedDataViewModel()
        {
            var competentAuthorities = fixture.CreateMany<UKCompetentAuthorityData>().ToList();
            var panAreas = fixture.CreateMany<PanAreaData>().ToList();
            var localAreas = fixture.CreateMany<LocalAreaData>().ToList();
            var years = fixture.CreateMany<int>().ToList();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).Returns(years);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).Returns(panAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetLocalAreas>._)).Returns(localAreas);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).Returns(competentAuthorities);

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfObligatedData(new AatfObligatedDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "AatfObligatedData");

            var model = viewResult.Model as AatfObligatedDataViewModel;
            Assert.NotNull(model);

            model.ComplianceYears.Select(c => c.Text).Should().BeEquivalentTo(years.Select(y => y.ToString()));
            model.PanAreaList.Select(c => c.Text).Should().BeEquivalentTo(panAreas.Select(y => y.Name.ToString()));
            model.PanAreaList.Select(c => c.Value).Should().BeEquivalentTo(panAreas.Select(y => y.Id.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Text).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Abbreviation.ToString()));
            model.CompetentAuthoritiesList.Select(c => c.Value).Should().BeEquivalentTo(competentAuthorities.Select(y => y.Id.ToString()));
        }

        /// <summary>
        /// This test ensures that the POST "AatfObligatedData" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfObligatedData_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfObligatedData(new AatfObligatedDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "AatfObligatedData" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfObligatedData_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Act
            var result = await controller.AatfObligatedData(new AatfObligatedDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "AatfObligatedData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfObligatedData_Always_SetsInternalBreadcrumbToViewReports()
        {
            await controller.AatfObligatedData(A.Dummy<AatfObligatedDataViewModel>());

            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        [Fact]
        public async Task GetAatfNonObligatedData_Always_ReturnsAatfAeReturnDataViewModel()
        {
            var years = fixture.CreateMany<int>().ToList();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).Returns(years);

            var result = await controller.AatfNonObligatedData();

            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().BeNullOrEmpty();

            var model = viewResult.Model as NonObligatedWeeeReceivedAtAatfViewModel;

            model.ComplianceYears.Select(c => c.Text).Should().BeEquivalentTo(years.Select(y => y.ToString()));

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task GetAatfNonObligatedData_GivenModelStateIsInvalid_SetsTriggerDownloadToFalse()
        {
            controller.ModelState.AddModelError("error", "error");

            var result = await controller.AatfNonObligatedData();

            var viewResult = result as ViewResult;

            viewResult.Should().NotBeNull();
            bool.FalseString.Should().BeSameAs(viewResult.ViewBag.TriggerDownload.ToString());
        }

        [Fact]
        public async Task GetAatfNonObligatedData_GivenModelStateIsValid_SetsTriggerDownloadToTrue()
        {
            var result = await controller.AatfNonObligatedData(new NonObligatedWeeeReceivedAtAatfViewModel()
            {
                SelectedYear = 2019
            });

            var viewResult = result as ViewResult;

            viewResult.Should().NotBeNull();
            bool.TrueString.Should().BeSameAs(viewResult.ViewBag.TriggerDownload.ToString());
        }

        [Fact]
        public async Task GetAatfNonObligatedData_Always_SetsInternalBreadcrumbToViewReports()
        {
            await controller.AatfNonObligatedData();

            "View reports".Should().Be(breadcrumb.InternalActivity);
        }

        [Fact]
        public async Task PostAatfNonObligatedData_Always_SetsInternalBreadcrumbToViewReports()
        {
            await controller.AatfNonObligatedData(new NonObligatedWeeeReceivedAtAatfViewModel()
            {
                SelectedYear = 2019
            });

            await controller.AatfNonObligatedData();

            "View reports".Should().Be(breadcrumb.InternalActivity);
        }

        [Fact]
        public async Task PostAatfNonObligatedData_WithInvalidViewModel_ReturnsNonObligatedWeeeReceivedAtAatfViewModel()
        {
            var years = fixture.CreateMany<int>().ToList();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfReturnsActiveComplianceYears>._)).Returns(years);

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfNonObligatedData(new NonObligatedWeeeReceivedAtAatfViewModel());

            // Assert
            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().BeNullOrEmpty();

            var model = viewResult.Model as NonObligatedWeeeReceivedAtAatfViewModel;

            model.ComplianceYears.Select(c => c.Text).Should().BeEquivalentTo(years.Select(y => y.ToString()));
        }

        [Fact]
        public async Task GetDownloadAatfNonObligatedDataCsv_GivenActionParameters_CsvShouldBeReturned()
        {
            const int complianceYear = 2019;
            const string aatName = "aatf";

            var fileData = fixture.Create<CSVFileData>();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUkNonObligatedWeeeReceivedAtAatfsDataCsv>.That.Matches(g =>
                g.AatfName.Equals(aatName)))).Returns(fileData);

            var result = await controller.DownloadAatfNonObligatedDataCsv(complianceYear, aatName) as FileContentResult;

            result.FileContents.Should().Contain(new UTF8Encoding().GetBytes(fileData.FileContent));
            result.FileDownloadName.Should().Be(CsvFilenameFormat.FormatFileName(fileData.FileName));
            result.ContentType.Should().Be("text/csv");
        }

        [Fact]
        public async Task GetAatfSentOnData_Always_ReturnsAatfSentOnDataViewModel()
        {
            IList<UKCompetentAuthorityData> competentAuthorities = fixture.CreateMany<UKCompetentAuthorityData>().ToList();
            IList<PanAreaData> panAreas = fixture.CreateMany<PanAreaData>().ToList();
            IList<LocalAreaData> localAreas = fixture.CreateMany<LocalAreaData>().ToList();
            // Arrange
            var years = new List<int>() { 2019 };
            A.CallTo(() => weeeClient.SendAsync(A<string>.Ignored, A<GetAatfReturnsActiveComplianceYears>.Ignored)).Returns(years);

            // Act
            var result = await controller.AatfSentOnData();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "AatfSentOnData");

            var model = viewResult.Model as AatfSentOnDataViewModel;
            Assert.NotNull(model);

            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2019", y1.Text));

            Assert.Collection(model.ObligationTypes,
                s1 => Assert.Equal("B2B", s1.Text),
                s2 => Assert.Equal("B2C", s2.Text));

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetPanAreas>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        /// <summary>
        /// This test ensures that the GET "AatfSentOnData" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAatfSentOnData_Always_SetsTriggerDownloadToFalse()
        {
            // Act
            var result = await controller.AatfSentOnData();

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "AatfSentOnData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAatfSentOnData_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.AatfSentOnData();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "AatfSentOnData" action with an invalid view model
        /// calls the API to retrieve the list of compliance years and returns the "AatfSentOnData"
        /// view with a AatfSentOnDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfSentOnData_WithInvalidViewModel_ReturnsAatfSentOnDataViewModel()
        {
            // Arrange
            var years = new List<int>() { 2019 };
            A.CallTo(() => weeeClient.SendAsync(A<string>.Ignored, A<GetAatfReturnsActiveComplianceYears>.Ignored)).Returns(years);

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfSentOnData(new AatfSentOnDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "AatfSentOnData");

            var model = viewResult.Model as AatfSentOnDataViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2019", y1.Text));
        }

        /// <summary>
        /// This test ensures that the POST "AatfSentOnData" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfSentOnData_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Act
            controller.ModelState.AddModelError("Key", "Error");
            var result = await controller.AatfSentOnData(new AatfSentOnDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "AatfSentOnData" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfSentOnData_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Act
            var result = await controller.AatfSentOnData(new AatfSentOnDataViewModel());

            // Assert
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "AatfSentOnData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostAatfSentOnData_Always_SetsInternalBreadcrumbToViewReports()
        {
            // Act
            await controller.AatfSentOnData(A.Dummy<AatfSentOnDataViewModel>());

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        [Fact]
        public async Task GetDownloadAatfSentOnDataCsv_ReturnsFileResultWithContentTypeOfTextCsv()
        {
            const int complianceYear = 2015;
            const string obligation = "b2c";
            const string aatf = "aatf";
            var authority = fixture.Create<Guid>();
            var panArea = fixture.Create<Guid>();
            var csvData = new CSVFileData
            {
                FileContent = "AatfSentOnDataCsv",
                FileName = "test.csv"
            };

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAllAatfSentOnDataCsv>.That.Matches(g => g.AATFName.Equals(aatf)
            && g.AuthorityId.Equals(authority) && g.ComplianceYear.Equals(complianceYear) && g.ObligationType.Equals(obligation) && g.PanArea.Equals(panArea)))).Returns(csvData);

            var fileResult = await controller.DownloadAatfSentOnDataCsv(complianceYear, obligation, aatf, authority, panArea) as FileContentResult;

            fileResult.ContentType.Should().Be("text/csv");
            fileResult.FileContents.Should().Contain(new UTF8Encoding().GetBytes(csvData.FileContent));
            fileResult.FileDownloadName.Should().Be(CsvFilenameFormat.FormatFileName(csvData.FileName));
        }
    }
}
