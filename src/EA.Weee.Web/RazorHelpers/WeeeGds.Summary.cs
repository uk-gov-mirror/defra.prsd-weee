﻿namespace EA.Weee.Web.RazorHelpers
{
    using System;

    public class SummaryEventTrackingFunction<TModel> : IDisposable
    {
        private readonly WeeeGds<TModel> gdsHelper;

        public SummaryEventTrackingFunction(WeeeGds<TModel> gdsHelper, string summaryText, string htmlclass, string eventCategory, string eventAction, string eventLabel = null)
        {
            this.gdsHelper = gdsHelper;

            var html = string.Format(
                @"<summary class = ""{0}"" onclick=""if(this.getAttribute('aria-expanded') == 'true'){{{1}}}"">{2}</summary>", htmlclass, gdsHelper.EventTrackingFunction(eventCategory, eventAction, eventLabel), summaryText);

            gdsHelper.HtmlHelper.ViewContext.Writer.Write(html);
        }

        public void Dispose()
        {
            gdsHelper.HtmlHelper.ViewContext.Writer.Write("</summary>");
        }
    }
}