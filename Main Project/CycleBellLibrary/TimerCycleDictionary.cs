using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CycleBellLibrary
{
    [XmlRoot("TimerCycles")]
    public class TimerCycleDictionary : SortedDictionary<int, int>, IXmlSerializable
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
            //writer.WriteElementString("Count", this.Count.ToString());

            foreach (var key in Keys) {

                writer.WriteStartElement("Element");
                writer.WriteAttributeString("key", key.ToString());
                writer.WriteString(this[key].ToString());
                writer.WriteEndElement();
            }
        }
    }
}
