﻿namespace EA.Weee.Xml.Converter
{
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Core.Helpers;
    using Deserializer;
    using Schemas;

    public class XmlConverter : IXmlConverter
    {
        private readonly IWhiteSpaceCollapser whiteSpaceCollapser;
        private readonly IDeserializer deserializer;

        public XmlConverter(IWhiteSpaceCollapser whiteSpaceCollapser, IDeserializer deserializer)
        {
            this.whiteSpaceCollapser = whiteSpaceCollapser;
            this.deserializer = deserializer;
        }

        public XDocument Convert(byte[] data)
        {
            return XDocument.Parse(XmlToUtf8String(data), LoadOptions.SetLineInfo);          
        }

        public string XmlToUtf8String(byte[] data)
        {
            data = RemoveUtf8Bom(data);
            return Encoding.UTF8.GetString(data);
        }

        public schemeType Deserialize(XDocument xdoc)
        {
            var scheme = deserializer.Deserialize<schemeType>(xdoc);
            whiteSpaceCollapser.Collapse(scheme);
            scheme.MakeEmptyStringsNull();

            return scheme;
        }

        private byte[] RemoveUtf8Bom(byte[] data)
        {
            var utf8Bom = Encoding.UTF8.GetPreamble();

            if (data.Length < utf8Bom.Length)
            {
                return data;
            }

            if (utf8Bom.Where((t, i) => data[i] != t).Any())
            {
                return data;
            }

            var filteredData = new byte[data.Length - utf8Bom.Length];
            for (var i = 0; i < data.Length - utf8Bom.Length; i++)
            {
                filteredData[i] = data[i + utf8Bom.Length];
            }

            return filteredData;
        }
    }
}