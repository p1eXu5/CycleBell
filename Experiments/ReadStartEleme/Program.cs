using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReadStartEleme
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder output = new StringBuilder();

            String xmlString =
                @"<book name=''>
    <title>Pride And Prejudice</title>
    <price>19.95</price>
</book>";

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString))) {
                // Parse the XML document. 
                reader.Read();
                reader.ReadStartElement("book");
                reader.ReadStartElement("title");
                output.AppendLine("The content of the title element:  ");
                output.AppendLine(reader.ReadContentAsString());
                reader.ReadEndElement();
                reader.ReadStartElement("price");
                output.AppendLine("The content of the price element:  ");
                output.AppendLine(reader.ReadContentAsDouble().ToString());
                reader.ReadEndElement();
                reader.ReadEndElement();

            }

            Console.WriteLine(output.ToString());
            


        }
    }
}
