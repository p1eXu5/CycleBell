using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CycleBellLibrary;

namespace TimerCycleDictionarySerialization
{
    class TimerCycleDictionarySerialization
    {
        static void Main(string[] args)
        {
            try {
                #region Arrange

                TimerCycleSortedDictionary tcd = new TimerCycleSortedDictionary();

                tcd[1] = 10;
                tcd[2] = 20;
                tcd[3] = 30;
                tcd[4] = 40;

                TimerCycleSortedDictionary outDict;

                #endregion


                // Serializing
                //using (FileStream fs = File.Open("TimerCycleDictionary.xml", FileMode.Create)) {

                //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(TimerCycleDictionary));
                //    xmlSerializer.Serialize(fs, tcd);
                //}

                // Deserializing
                using (FileStream fs = File.Open("TimerCycleDictionary.xml", FileMode.Open)) {

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(TimerCycleSortedDictionary));
                    outDict = (TimerCycleSortedDictionary)xmlSerializer.Deserialize(fs);
                }

                qqq:
                Console.WriteLine("Keys:");
                outDict.Keys.ToList().ForEach(i => Console.Write($"{i} "));
                Console.WriteLine("\n\nValues:");
                outDict.Values.ToList().ForEach(v => Console.Write($"{v} "));

                Console.WriteLine("\n\nDone!");
                Console.ReadKey(true);
            }
            catch (Exception ex) {

                Console.WriteLine($"{ex.Message}");
            }

            Console.WriteLine("sdssdssd");
            Console.ReadKey(true);
        }
    }
}
