﻿namespace EA.Weee.XmlValidation.BusinessValidation.QuerySets.Queries
{
    public interface IQuery<out T>
    {
        T Run();
    }
}
