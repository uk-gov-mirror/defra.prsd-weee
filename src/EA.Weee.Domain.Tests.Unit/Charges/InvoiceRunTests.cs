﻿namespace EA.Weee.Domain.Tests.Unit.Charges
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Charges;
    using Domain.Scheme;
    using FakeItEasy;
    using Xunit;

    public class InvoiceRunTests
    {
        /// <summary>
        /// This test ensures that an invoice run cannot be constructed with zero member uploads.
        /// </summary>
        [Fact]
        public void Constructor_WithNoMemberUploads_ThrowsInvalidOperationException()
        {
            // Arrange
            List<MemberUpload> emptyListOfMemberUploads = new List<MemberUpload>();

            // Act
            Func<InvoiceRun> testCode = () => new InvoiceRun(A.Dummy<UKCompetentAuthority>(), emptyListOfMemberUploads);

            // Assert
            Assert.Throws<InvalidOperationException>(testCode);
        }

        /// <summary>
        /// This test ensures that all member uploads in an invoice run must relate to schemes for the same
        /// appropriate authority as the invoice run.
        /// </summary>
        [Fact]
        public void Constructor_WithMemberUploadForDifferentAuthority_ThrowsInvalidOperationException()
        {
            // Arrange
            UKCompetentAuthority authorityA = A.Dummy<UKCompetentAuthority>();
            UKCompetentAuthority authorityB = A.Dummy<UKCompetentAuthority>();

            Scheme schemeForAuthorityA = A.Fake<Scheme>();
            A.CallTo(() => schemeForAuthorityA.CompetentAuthority).Returns(authorityA);

            List<MemberUpload> memberUploads = new List<MemberUpload>();

            MemberUpload memberUploadForAuthorityA = new MemberUpload(
                new Guid("A2A01A99-A97D-4219-9060-D7CDF7435114"),
                schemeForAuthorityA,
                "data",
                "filename");
            memberUploads.Add(memberUploadForAuthorityA);

            // Act
            Func<InvoiceRun> testCode = () => new InvoiceRun(authorityB, memberUploads);

            // Assert
            Assert.Throws<InvalidOperationException>(testCode);
        }

        /// <summary>
        /// This test ensures that a member upload which has already been assigned to an invoice run
        /// cannot be assigned to another invoice run.
        /// </summary>
        [Fact]
        public void Constructor_WithMemberUploadAlreadyAssignedToInvoiceRun_ThrowsInvalidOperationException()
        {
            // Arrange
            UKCompetentAuthority authority = A.Dummy<UKCompetentAuthority>();

            Scheme scheme = A.Fake<Scheme>();
            A.CallTo(() => scheme.CompetentAuthority).Returns(authority);

            List<MemberUpload> memberUploads = new List<MemberUpload>();
            MemberUpload memberUpload1 = new MemberUpload(new Guid("A2A01A99-A97D-4219-9060-D7CDF7435114"), scheme, "data", "filename");
            memberUploads.Add(memberUpload1);

            InvoiceRun invoiceRun = new InvoiceRun(authority, memberUploads);

            // Act
            Func<InvoiceRun> testCode = () => new InvoiceRun(authority, memberUploads);

            // Assert
            Assert.Throws<InvalidOperationException>(testCode);
        }

        /// <summary>
        /// This test ensures that the member uploads are assigned to the invoice run being
        /// constructed. 
        /// </summary>
        [Fact]
        public void Constructor_Always_AssignsMemberUploadsToInvoiceRun()
        {
            // Arrange
            UKCompetentAuthority authority = A.Dummy<UKCompetentAuthority>();

            Scheme scheme = A.Fake<Scheme>();
            A.CallTo(() => scheme.CompetentAuthority).Returns(authority);

            List<MemberUpload> memberUploads = new List<MemberUpload>();
            MemberUpload memberUpload1 = new MemberUpload(new Guid("A2A01A99-A97D-4219-9060-D7CDF7435114"), scheme, "data", "filename");
            memberUploads.Add(memberUpload1);

            // Act
            InvoiceRun result = new InvoiceRun(authority, memberUploads);

            // Assert
            Assert.Equal(result, memberUpload1.InvoiceRun);
        }
    }
}