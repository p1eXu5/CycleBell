using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CycleBellLibrary.Repository
{
    /// <summary>
    /// For serialization
    /// </summary>
    [XmlRoot("TimerLoops")]
    public class TimerLoopSortedDictionary : SortedDictionary<int, int>, IXmlSerializable
    {
        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            try {
                reader.ReadStartElement();

                if (reader.IsEmptyElement)
                    return;

                while (reader.NodeType != XmlNodeType.EndElement) {

                    int key = Int32.Parse(reader.GetAttribute("key") ?? "0");
                    this[key] = reader.ReadElementContentAsInt();
                }

                reader.ReadEndElement();
            }
            catch (Exception ex) {
                this.Clear();
                throw;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in Keys) {

                writer.WriteStartElement("Element");
                writer.WriteAttributeString("key", key.ToString());
                writer.WriteString(this[key].ToString());
                writer.WriteEndElement();
            }
        }
    }
}
