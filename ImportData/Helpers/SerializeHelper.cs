using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace ImportData.Helpers
{
    public static class SerializeHelper
    {
        public static byte[] Serialize<T>(T item) where T : class
        {
            var dc = new DataContractSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                dc.WriteObject(stream, item);
                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] data) where T : class
        {
            var dc = new DataContractSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream(data))
            {
                return (T)dc.ReadObject(stream);
            }
        }

        public static byte[] SerializeArray<T>(this T[] list) where T : class
        {
            var dc = new DataContractSerializer(typeof(T[]));
            using (MemoryStream stream = new MemoryStream())
            {
                dc.WriteObject(stream, list);
                return stream.ToArray();
            }
        }

        public static T[] DeserializeArray<T>(this byte[] data) where T : class
        {
            var dc = new DataContractSerializer(typeof(T[]));
            using (MemoryStream stream = new MemoryStream(data))
            {
                return (T[])dc.ReadObject(stream);
            }
        }

        public static string XmlSerializeObject<T>(T obj) where T : class
        {
            StringBuilder XmlizedString = new StringBuilder();

            XmlSerializer xs = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            XmlWriter xmlTextWriter = XmlTextWriter.Create(XmlizedString, settings);
            xs.Serialize(xmlTextWriter, obj);

            return XmlizedString.ToString();
        }

        public static T XmlDeserializeObject<T>(string xml) where T : class
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(xml));
            return (T)xs.Deserialize(xmlTextReader);
        }
    }
}
