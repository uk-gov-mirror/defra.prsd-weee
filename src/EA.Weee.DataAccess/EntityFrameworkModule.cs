﻿namespace EA.Weee.DataAccess
{
    using Autofac;
    using DataAccess;
    using EA.Prsd.Core.Domain;

    public class EntityFrameworkModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WeeeContext>().AsSelf().InstancePerRequest();

            builder.RegisterAssemblyTypes(ThisAssembly).AsClosedTypesOf(typeof(IEventHandler<>));

            builder.RegisterType<RegisteredProducerDataAccess>().As<IRegisteredProducerDataAccess>()
                .InstancePerRequest();

            builder.RegisterType<QuarterWindowTemplateDataAccess>().As<IQuarterWindowTemplateDataAccess>()
                .InstancePerRequest();
        }
    }
}