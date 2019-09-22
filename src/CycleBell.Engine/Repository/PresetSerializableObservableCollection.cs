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
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Repository
{
    /// <summary>
    /// For serialization <see cref="Preset"/><seealso cref="ObservableCollection{T}"/>
    /// </summary>
    [XmlRoot("PresetCollection")]
    internal class PresetSerializableObservableCollection : ObservableCollection< Preset >, IXmlSerializable
    {
        public virtual XmlSchema GetSchema() => null;

        public virtual void ReadXml(XmlReader reader)
        {
            XmlSerializer timersCyclesSerializer = new XmlSerializer(typeof(TimerLoopSerializableSortedDictionary));

            try {
                // Читает текущий элемент с заданным именем и смещает указатель к следующему элементу
                // иначе бросает исключение
                reader.ReadStartElement("PresetCollection");

                if (reader.IsEmptyElement)
                    return;

                while (reader.NodeType != XmlNodeType.EndElement) {

                    Preset preset = new Preset();

                    // <Preset name=_ >
                    preset.PresetName = reader.GetAttribute("name");
                    reader.Read();

                    // <StartTime> (ReadElementContentAs... - читает и перекидывает на следующий элемент)

                    preset.StartTime = TimeSpan.Parse(reader.ReadElementContentAsString());
                    // </StartrTime>

                    // <InfiniteLoop> (Boolean - это "0", либо "1")
                    if (reader.ReadElementContentAsBoolean())
                        preset.SetInfiniteLoop();
                    // </InfiniteLoop>

                    // <Tag>
                    if (!reader.IsEmptyElement) {
                        preset.Tag = reader.ReadElementContentAsString();
                    }
                    else {
                        reader.Read();
                    }
                    // </Tag

                    if (!reader.IsEmptyElement) {

                        reader.ReadStartElement("TimePointCollection");

                        var timePoints = new List<TimePoint>();

                        // <TimePointCollection>
                        while (reader.NodeType != XmlNodeType.EndElement) {

                            TimePoint tp = new TimePoint();
                            tp.Name = reader.GetAttribute("name");
                            reader.Read();
                            tp.Time = TimeSpan.Parse(reader.ReadElementContentAsString());
                            tp.ChangeTimePointType((TimePointKinds) (reader.ReadElementContentAsInt()));
                            tp.LoopNumber = (byte) (reader.ReadElementContentAsInt());

                            if (reader.IsEmptyElement)
                                reader.Read();
                            else {
                                tp.Tag = reader.ReadElementContentAsString();
                            }

                            reader.ReadEndElement();

                            timePoints.Add(tp);
                        }

                        // </TimePointCollection>
                        reader.ReadEndElement();

                        preset.AddTimePoints(timePoints);
                    }
                    else {
                        reader.Read();
                    }

                    // <TimerLoopDictionary>
                    if (!reader.IsEmptyElement) {
                        preset.TimerLoopDictionary = (TimerLoopSerializableSortedDictionary) timersCyclesSerializer.Deserialize(reader);
                    }
                    else {
                        reader.Read();
                    }
                    // </TimerLoopDictionary>

                    // </Preset>
                    reader.ReadEndElement();

                    this.Add(preset);
                }

                // </PresetCollection>
                reader.ReadEndElement();
            }
            catch {

                this.Clear();
                //throw;
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            XmlSerializer timersCyclesSerializer = new XmlSerializer(typeof(TimerLoopSerializableSortedDictionary));

            for (int i = 0; i < this.Count; ++i) {

                writer.WriteStartElement("Preset");
                writer.WriteAttributeString("name", this[i].PresetName);

                writer.WriteElementString("StartTime", this[i].StartTime.ToString(@"h\:mm"));
                writer.WriteElementString("IsInfiniteLoop", this[i].IsInfiniteLoop ? "1" : "0");
                writer.WriteElementString("Tag", this[i].Tag == null ? "" : this[i].Tag.ToString());

                writer.WriteStartElement("TimePointCollection");

                for (int j = 0; j < this[i].TimePointCollection.Count; ++j) {

                    writer.WriteStartElement("TimePoint");
                    writer.WriteAttributeString("name", this[i].TimePointCollection[j].Name);
                    writer.WriteElementString("Time", this[i].TimePointCollection[j].Time.ToString(@"h\:mm\:ss"));
                    writer.WriteElementString("Kind",
                                              ((byte) (this[i].TimePointCollection[j].Kind)).ToString());
                    writer.WriteElementString("CycleNum", this[i].TimePointCollection[j].LoopNumber.ToString());
                    writer.WriteElementString("SoundLocation", (string) this[i].TimePointCollection[j].Tag ?? "");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                // <TimerLoopDictionary>
                timersCyclesSerializer.Serialize(writer,this[i].TimerLoopDictionary);
                // </TimerLoopDictionary>

                writer.WriteEndElement();
            }
        }
    }
}
