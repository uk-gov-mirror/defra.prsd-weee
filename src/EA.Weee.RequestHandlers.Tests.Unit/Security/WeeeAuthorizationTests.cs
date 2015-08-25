﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Security
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Security;
    using System.Security.Claims;
    using Core;
    using DataAccess;
    using Domain;
    using Domain.Organisation;
    using FakeItEasy;
    using Helpers;
    using Prsd.Core.Domain;
    using RequestHandlers.Security;
    using Xunit;

    public class WeeeAuthorizationTests
    {
        [Fact]
        [Trait("Area", "Security")]
        public void EnsureCanAccessInternalArea_ThrowsSecurityException_WhenUserHasNoClaims()
        {
            // Arrange
            WeeeContext weeeContext = A.Fake<WeeeContext>();

            ClaimsPrincipal principal = new ClaimsPrincipal();
            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.Principal).Returns(principal);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            Action action = () => authorization.EnsureCanAccessInternalArea();

            // Assert
            Assert.Throws(typeof(SecurityException), action);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckCanAccessInternalArea_ReturnsFalse_WhenUserHasNoClaims()
        {
            // Arrange
            WeeeContext weeeContext = A.Fake<WeeeContext>();

            ClaimsPrincipal principal = new ClaimsPrincipal();
            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.Principal).Returns(principal);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckCanAccessInternalArea();

            // Assert
            Assert.Equal(false, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckCanAccessInternalArea_ReturnsTrue_WhenUserHasRequiredClaim()
        {
            // Arrange
            WeeeContext weeeContext = A.Fake<WeeeContext>();

            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, Claims.CanAccessInternalArea));
            
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.Principal).Returns(principal);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckCanAccessInternalArea();

            // Assert
            Assert.Equal(true, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void EnsureCanAccessExternalArea_ThrowsSecurityException_WhenUserHasNoClaims()
        {
            // Arrange
            WeeeContext weeeContext = A.Fake<WeeeContext>();

            ClaimsPrincipal principal = new ClaimsPrincipal();
            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.Principal).Returns(principal);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            Action action = () => authorization.EnsureCanAccessExternalArea();

            // Assert
            Assert.Throws(typeof(SecurityException), action);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckCanAccessExternalArea_ReturnsFalse_WhenUserHasNoClaims()
        {
            // Arrange
            WeeeContext weeeContext = A.Fake<WeeeContext>();

            ClaimsPrincipal principal = new ClaimsPrincipal();
            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.Principal).Returns(principal);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckCanAccessExternalArea();

            // Assert
            Assert.Equal(false, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckCanAccessExternalArea_ReturnsTrue_WhenUserHasRequiredClaim()
        {
            // Arrange
            WeeeContext weeeContext = A.Fake<WeeeContext>();

            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, Claims.CanAccessExternalArea));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.Principal).Returns(principal);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckCanAccessExternalArea();

            // Assert
            Assert.Equal(true, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void EnsureOrganisationAccess_ThrowsSecurityException_WhenUserHasNoClaims()
        {
            // Arrange
            Guid organisationID = new Guid("5F3069F4-EDA3-43A3-BDD8-726028CDABB0");
            Guid userId = Guid.NewGuid();

            WeeeContext weeeContext = MakeFakeWeeeContext();

            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.UserId).Returns(userId);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            Action action = () => authorization.EnsureOrganisationAccess(organisationID);

            // Assert
            Assert.Throws(typeof(SecurityException), action);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckOrganisationAccess_ReturnsFalse_WhenUserHasNoClaims()
        {
            // Arrange
            Guid organisationID = new Guid("5F3069F4-EDA3-43A3-BDD8-726028CDABB0");
            Guid userId = Guid.NewGuid();

            WeeeContext weeeContext = MakeFakeWeeeContext();

            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.UserId).Returns(userId);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckOrganisationAccess(organisationID);

            // Assert
            Assert.Equal(false, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckOrganisationAccess_ReturnsTrue_WhenUserHasRequiredClaims()
        {
            // Arrange
            Guid organisationID = new Guid("5F3069F4-EDA3-43A3-BDD8-726028CDABB0");
            Guid userId = Guid.NewGuid();

            WeeeContext weeeContext =
                MakeFakeWeeeContext(
                    new List<OrganisationUser> { new OrganisationUser(userId, organisationID, UserStatus.Active) });

            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.UserId).Returns(userId);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckOrganisationAccess(organisationID);

            // Assert
            Assert.Equal(true, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void EnsureSchemeAccess_ThrowsSecurityException_WhenUserHasNoClaims()
        {
            // Arrange
            Guid schemeID = new Guid("5F3069F4-EDA3-43A3-BDD8-726028CDABB0");
            Guid userId = Guid.NewGuid();

            WeeeContext weeeContext = MakeFakeWeeeContext();

            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.UserId).Returns(userId);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            Action action = () => authorization.EnsureSchemeAccess(schemeID);

            // Assert
            Assert.Throws(typeof(SecurityException), action);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckSchemeAccess_ReturnsFalse_WhenUserHasNoClaims()
        {
            // Arrange
            Guid schemeID = new Guid("5F3069F4-EDA3-43A3-BDD8-726028CDABB0");
            Guid userId = Guid.NewGuid();

            WeeeContext weeeContext = MakeFakeWeeeContext();

            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.UserId).Returns(userId);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckSchemeAccess(schemeID);

            // Assert
            Assert.Equal(false, result);
        }

        [Fact]
        [Trait("Area", "Security")]
        public void CheckSchemeAccess_ReturnsTrue_WhenUserHasRequiredClaims()
        {
            // Arrange
            Guid organisationID = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid schemeID = new Guid("5F3069F4-EDA3-43A3-BDD8-726028CDABB0");
            Domain.Scheme.Scheme scheme = new Domain.Scheme.Scheme(organisationID);
            typeof(Entity).GetProperty("Id").SetValue(scheme, schemeID); // <- sad but necessary

            WeeeContext weeeContext =
                MakeFakeWeeeContext(
                    new List<OrganisationUser> { new OrganisationUser(userId, organisationID, UserStatus.Active) },
                    new List<Domain.Scheme.Scheme> { scheme });

            IUserContext userContext = A.Fake<IUserContext>();
            A.CallTo(() => userContext.UserId).Returns(userId);

            WeeeAuthorization authorization = new WeeeAuthorization(weeeContext, userContext);

            // Act
            bool result = authorization.CheckSchemeAccess(schemeID);

            // Assert
            Assert.Equal(true, result);
        }

        private WeeeContext MakeFakeWeeeContext(List<OrganisationUser> organisationUsers = null, List<Domain.Scheme.Scheme> schemes = null)
        {
            organisationUsers = organisationUsers ?? new List<OrganisationUser>();
            schemes = schemes ?? new List<Domain.Scheme.Scheme>();

            var dbHelper = new DbContextHelper();
            WeeeContext weeeContext = A.Fake<WeeeContext>();
            A.CallTo(() => weeeContext.OrganisationUsers).Returns(dbHelper.GetAsyncEnabledDbSet(organisationUsers));
            A.CallTo(() => weeeContext.Schemes).Returns(dbHelper.GetAsyncEnabledDbSet(schemes));

            return weeeContext;
        }
    }
}
