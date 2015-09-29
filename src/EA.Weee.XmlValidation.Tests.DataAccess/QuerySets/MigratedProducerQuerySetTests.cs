﻿namespace EA.Weee.XmlValidation.Tests.DataAccess.BusinessValidation.Rules.QuerySets
{
    using DataAccess;
    using EA.Weee.Tests.Core.Model;
    using FakeItEasy;
    using System;
    using System.Collections.Generic;
    using Weee.Tests.Core;
    using XmlValidation.BusinessValidation.QuerySets;
    using Xunit;

    public class MigratedProducerQuerySetTests
    {
        [Fact]
        public void GetMigratedProducer_PrnDoesNotExist_ReturnsNull()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                MigratedProducerQuerySet querySet = new MigratedProducerQuerySet(database.WeeeContext);

                // Act
                EA.Weee.Domain.Producer.MigratedProducer result = querySet.GetMigratedProducer("XXX");

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public void GetMigratedProducer_PrnExists_ReturnsMigratedProducer()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                MigratedProducer migratedProducer1 = new MigratedProducer();
                migratedProducer1.Id = new Guid("B1750CCE-77D9-4A11-9ACC-D29D692FAA52");
                migratedProducer1.ProducerRegistrationNumber = "ABC";
                migratedProducer1.ProducerName = "Test Producer 1";

                database.Model.MigratedProducers.Add(migratedProducer1);
                database.Model.SaveChanges();

                MigratedProducerQuerySet querySet = new MigratedProducerQuerySet(database.WeeeContext);

                // Act
                EA.Weee.Domain.Producer.MigratedProducer result = querySet.GetMigratedProducer("ABC");

                // Assert
                Assert.NotNull(result);
                Assert.Equal(new Guid("B1750CCE-77D9-4A11-9ACC-D29D692FAA52"), result.Id);
                Assert.Equal("Test Producer 1", result.ProducerName);
            }
        }
    }
}