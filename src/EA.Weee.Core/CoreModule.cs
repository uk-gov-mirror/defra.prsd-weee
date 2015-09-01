﻿namespace EA.Weee.Core
{
    using Autofac;
    using Autofac.Core;
    using Configuration.EmailRules;
    using EA.Weee.Core.Shared;
    using XmlBusinessValidation;

    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Email Rules
            builder.RegisterType<RuleSectionChecker>().As<IRuleSectionChecker>();
            builder.RegisterType<RuleChecker>().As<IRuleChecker>();

            // Register the helper classes
            builder.RegisterAssemblyTypes(this.GetType().Assembly)
                .Where(t => t.Namespace.Contains("Helpers"))
                .AsImplementedInterfaces();

            builder.RegisterType<NoFormulaeExcelSanitizer>().As<IExcelSanitizer>();

            builder.RegisterType<CsvWriterFactory>().SingleInstance();

            // XML rules
            builder.RegisterType<RuleSelector>().As<IRuleSelector>();
        }
    }
}
