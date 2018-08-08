﻿using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CycleBellLibrary
{
    [XmlRoot("Presets")]
    public class PresetObservableCollection : ObservableCollection<Preset>, IXmlSerializable
    {
        public virtual XmlSchema GetSchema() => null;

        public virtual void ReadXml(XmlReader reader)
        {
            try {
                // Читает текущий элемент с заданным именем и смещает указатель к следующему элементу
                // иначе бросает исключение
                reader.ReadStartElement("Presets");

                if (reader.IsEmptyElement)
                    return;

                while (reader.NodeType != XmlNodeType.EndElement) {

                    Preset preset = new Preset();

                    preset.PresetName = reader.GetAttribute("name");
                    reader.Read();

                    // ReadElementContent... - читает и перекидывает на следующий элемент
                    preset.StartTime = TimeSpan.Parse(reader.ReadElementContentAsString());

                    // Boolean - это "0", либо "1"
                    if (reader.ReadElementContentAsBoolean())
                        preset.SetInfiniteLoop();

                    reader.ReadStartElement("TimePoints");

                    // TimePoints read loop
                    while (reader.NodeType != XmlNodeType.EndElement) {

                        TimePoint tp = new TimePoint();
                        tp.Name = reader.GetAttribute("name");
                        reader.Read();
                        tp.Time = TimeSpan.Parse(reader.ReadElementContentAsString());
                        tp.TimePointType = (TimePointType)(reader.ReadElementContentAsInt());
                        tp.TimerCycleNum = (byte)(reader.ReadElementContentAsInt());

                        if (reader.IsEmptyElement)
                            reader.Read();
                        else {
                            tp.Tag = reader.ReadElementContentAsString();
                        }

                        reader.ReadEndElement();

                        preset.AddTimePoint(tp);
                    }

                    // </TimePoints>
                    reader.ReadEndElement();

                    // </Preset>
                    reader.ReadEndElement();

                    this.Add(preset);
                }

                // </Presets>
                reader.ReadEndElement();
            }
            catch (XmlException ex) {

                this.Clear();
                throw;
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            //XmlSerializer timersCyclesSerializer = new XmlSerializer(typeof(TimerCycleDictionary));

            for (int i = 0; i < this.Count; ++i) {

                writer.WriteStartElement("Preset");
                writer.WriteAttributeString("name", this[i].PresetName);

                writer.WriteElementString("StartTime", this[i].StartTime.ToString(@"h\:mm"));
                writer.WriteElementString("IsInfiniteLoop", this[i].IsInfiniteLoop ? "1" : "0");

                writer.WriteStartElement("TimePoints");

                for (int j = 0; j < this[i].TimePoints.Count; ++j) {

                    writer.WriteStartElement("TimePoint");
                    writer.WriteAttributeString("name", this[i].TimePoints[j].Name);
                    writer.WriteElementString("Time", this[i].TimePoints[j].Time.ToString(@"h\:mm\:ss"));
                    writer.WriteElementString("TimePointType",
                                              ((byte) (this[i].TimePoints[j].TimePointType)).ToString());
                    writer.WriteElementString("CycleNum", this[i].TimePoints[j].TimerCycleNum.ToString());
                    writer.WriteElementString("SoundLocation", (string) this[i].TimePoints[j].Tag ?? "");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                //timersCyclesSerializer.Serialize(writer,this[0].TimersCycles);

                writer.WriteEndElement();
            }
        }
    }
}
