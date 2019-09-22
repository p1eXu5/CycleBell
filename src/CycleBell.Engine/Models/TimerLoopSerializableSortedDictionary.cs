/*
 * Copyright © 2018 Vladimir Likhatskiy. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *  
 */

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CycleBell.Engine.Models
{
    /// <summary>
    /// For serialization
    /// </summary>
    [XmlRoot("TimerLoopDictionary")]
    public class TimerLoopSerializableSortedDictionary : SortedDictionary<int, int>, IXmlSerializable
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
            catch {
                Clear();
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
