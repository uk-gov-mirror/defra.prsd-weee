﻿namespace EA.Weee.RequestHandlers.Tests.Unit.DataReturns.ProcessDataReturnXmlFile
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Shared;
    using Domain.DataReturns;
    using Domain.Lookup;
    using FakeItEasy;
    using RequestHandlers.DataReturns.ProcessDataReturnXmlFile;
    using RequestHandlers.DataReturns.ReturnVersionBuilder;
    using Xml.DataReturns;
    using Xunit;
    using ObligationType = Domain.ObligationType;
    using Scheme = Domain.Scheme.Scheme;

    public class DataReturnVersionFromXmlBuilderTests
    {
        [Fact]
        public async Task Build_NoProducers_DoesNotProcessProducerList()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                ProducerList = null
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddEeeOutputAmount(A<string>._, A<string>._, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustNotHaveHappened();
        }

        [Fact]
        public async Task Build_ProcessesAllReturnsInProducerList()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                ProducerList = new SchemeReturnProducer[]
                {
                    new SchemeReturnProducer
                    {
                        Return = new[] { A.Dummy<TonnageReturnType>(), A.Dummy<TonnageReturnType>() }
                    },
                    new SchemeReturnProducer
                    {
                        Return = new[] { A.Dummy<TonnageReturnType>() }
                    }
                }
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddEeeOutputAmount(A<string>._, A<string>._, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustHaveHappened(Repeated.Exactly.Times(3));
        }

        [Fact]
        public async Task Build_NoCollectedFromDcf_DoesNotProcessCollectedFromDcf()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                CollectedFromDCF = null
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddWeeeCollectedAmount(WeeeCollectedAmountSourceType.Dcf, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustNotHaveHappened();
        }

        [Fact]
        public async Task Build_ProcessesAllReturnsInCollectedFromDcf()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                CollectedFromDCF = new[] { A.Dummy<TonnageReturnType>(), A.Dummy<TonnageReturnType>() }
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddWeeeCollectedAmount(WeeeCollectedAmountSourceType.Dcf, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Fact]
        public async Task Build_NoB2CWEEEFromDistributors_DoesNotProcessB2CWEEEFromDistributors()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                B2CWEEEFromDistributors = null
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddWeeeCollectedAmount(WeeeCollectedAmountSourceType.Distributor, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustNotHaveHappened();
        }

        [Fact]
        public async Task Build_ProcessesAllReturnsInB2CWEEEFromDistributors()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                B2CWEEEFromDistributors = new[] { A.Dummy<TonnageReturnType>(), A.Dummy<TonnageReturnType>() }
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddWeeeCollectedAmount(WeeeCollectedAmountSourceType.Distributor, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Fact]
        public async Task Build_NoB2CWEEEFromFinalHolders_DoesNotProcessB2CWEEEFromFinalHolders()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                B2CWEEEFromFinalHolders = null
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddWeeeCollectedAmount(WeeeCollectedAmountSourceType.FinalHolder, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustNotHaveHappened();
        }

        [Fact]
        public async Task Build_ProcessesAllReturnsInB2CWEEEFromFinalHolders()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                B2CWEEEFromFinalHolders = new[] { A.Dummy<TonnageReturnType>(), A.Dummy<TonnageReturnType>() }
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddWeeeCollectedAmount(WeeeCollectedAmountSourceType.FinalHolder, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Fact]
        public async Task Build_NoDeliveredToAaTF_DoesNotProcessDeliveredToAaTF()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                DeliveredToATF = null
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddAatfDeliveredAmount(A<string>._, A<string>._, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustNotHaveHappened();
        }

        [Fact]
        public async Task Build_ProcessesAllReturnsInDeliveredToAaTF()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                DeliveredToATF = new[]
                {
                    new SchemeReturnDeliveredToATF
                    {
                        DeliveredToFacility = A.Dummy<SchemeReturnDeliveredToATFDeliveredToFacility>(),
                        Return = new[] { A.Dummy<TonnageReturnType>(), A.Dummy<TonnageReturnType>() }
                    },
                    new SchemeReturnDeliveredToATF
                    {
                        DeliveredToFacility = A.Dummy<SchemeReturnDeliveredToATFDeliveredToFacility>(),
                        Return = new[] { A.Dummy<TonnageReturnType>() }
                    },
                }
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddAatfDeliveredAmount(A<string>._, A<string>._, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustHaveHappened(Repeated.Exactly.Times(3));
        }

        [Fact]
        public async Task Build_NoDeliveredToAe_DoesNotProcessDeliveredToAe()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                DeliveredToAE = null
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddAeDeliveredAmount(A<string>._, A<string>._, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustNotHaveHappened();
        }

        [Fact]
        public async Task Build_ProcessesAllReturnsInDeliveredToAe()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH",
                DeliveredToAE = new[]
                {
                    new SchemeReturnDeliveredToAE
                    {
                        DeliveredToOperator = A.Dummy<SchemeReturnDeliveredToAEDeliveredToOperator>(),
                        Return = new[] { A.Dummy<TonnageReturnType>(), A.Dummy<TonnageReturnType>() }
                    },
                    new SchemeReturnDeliveredToAE
                    {
                        DeliveredToOperator = A.Dummy<SchemeReturnDeliveredToAEDeliveredToOperator>(),
                        Return = new[] { A.Dummy<TonnageReturnType>() }
                    },
                }
            };

            await builder.Create().Build(schemeReturn);

            A.CallTo(() => builder.DataReturnVersionBuilder.AddAeDeliveredAmount(A<string>._, A<string>._, A<WeeeCategory>._, A<ObligationType>._, A<decimal>._))
                 .MustHaveHappened(Repeated.Exactly.Times(3));
        }

        [Fact]
        public async Task Build_ReturnsResult()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var expectedResult = new DataReturnVersionBuilderResult(A.Dummy<DataReturnVersion>(), A.Dummy<List<ErrorData>>());

            A.CallTo(() => builder.DataReturnVersionBuilder.Build())
                 .Returns(expectedResult);

            SchemeReturn schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/AA1111AA/SCH"
            };

            var actualResult = await builder.Create().Build(schemeReturn);

            Assert.Equal(expectedResult, actualResult);
        }

        /// <summary>
        /// This test ensures that the scheme approval number provided in the XML file matches
        /// the approval number of the scheme for which the data return version is being built.
        /// If not, an "Error" level error should be returned with a description that contains
        /// the scheme approval number that was provided.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Build_DifferentSchemeApprovalNumber_ReturnsError()
        {
            var builder = new DataReturnVersionFromXmlBuilderHelper("WEE/AA1111AA/SCH");

            var schemeReturn = new SchemeReturn()
            {
                ApprovalNo = "WEE/ZZ9999ZZ/SCH",
            };

            DataReturnVersionBuilderResult result = await builder.Create().Build(schemeReturn);

            Assert.NotNull(result);
            Assert.Null(result.DataReturnVersion);
            Assert.NotEmpty(result.ErrorData);

            ErrorData firstError = result.ErrorData[0];

            Assert.NotNull(firstError);
            Assert.Equal(ErrorLevel.Error, firstError.ErrorLevel);
            Assert.Contains("WEE/ZZ9999ZZ/SCH", firstError.Description);
        }

        private class DataReturnVersionFromXmlBuilderHelper
        {
            public IDataReturnVersionBuilder DataReturnVersionBuilder;

            public DataReturnVersionFromXmlBuilderHelper(string schemeApprovalNumber)
            {
                DataReturnVersionBuilder = A.Fake<IDataReturnVersionBuilder>();

                Scheme scheme = new Scheme(new Guid("FE4056B3-F892-476E-A4AB-7C111AE1EF14"));

                scheme.UpdateScheme(
                    "Test scheme",
                    schemeApprovalNumber,
                    "1B1S",
                    ObligationType.Both,
                    new Guid("C5D400BE-0CE7-43D7-BD7B-B7936967E500"));

                A.CallTo(() => DataReturnVersionBuilder.Scheme).Returns(scheme);
            }

            public DataReturnVersionFromXmlBuilder Create()
            {
                return new DataReturnVersionFromXmlBuilder(DataReturnVersionBuilder);
            }
        }
    }
}