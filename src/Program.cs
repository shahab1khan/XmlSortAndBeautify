
namespace XmlCompareConsole
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    using NDesk.Options;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            string inputFile = null;
            string outputFile = null;

            var p = new OptionSet();
            p.Add("i=|inputfile", s => inputFile = s);
            p.Add("o=|outputfile", s => outputFile = s);
            p.Parse(args);

            try
            {
                using (StreamReader s = new StreamReader(inputFile))
                {
                    var encoding = s.CurrentEncoding;
                    var contents = s.ReadToEnd();
                    contents = XmlSortAndBeautify(contents);
                    Console.Write(contents);

                    if (outputFile != null)
                    {
                        using (StreamWriter sw = new StreamWriter(outputFile))
                        {
                            using (StreamWriter sw2 = new StreamWriter(sw.BaseStream, encoding))
                            {
                                sw2.Write(contents);
                            }
                        }
                    }

                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// The all attributes as string.
        /// </summary>
        /// <param name="attributes">
        /// The attributes.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string SortAndAggregateAttributesAsString(IEnumerable<XAttribute> attributes)
        {
            var sorted = attributes.OrderBy(x => x.Name.LocalName).ToList();

            return sorted.Aggregate(
                string.Empty,
                (s, attribute) => s + attribute.Name + attribute.Value ?? string.Empty);
        }

        /// <summary>
        /// The xml sort.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        private static void XmlSort(XElement element)
        {
            if (element.Elements().Count() <= 1)
            {
                return;
            }

            var orderedElements = element.Elements()
                .OrderBy(xElement => xElement.Name + SortAndAggregateAttributesAsString(xElement.Attributes())).ToList();

            foreach (XElement xElement in orderedElements)
            {
                XmlSort(xElement);
            }

            element.RemoveNodes();

            foreach (XElement xElement in orderedElements)
            {
                element.Add(xElement);
            }
        }

        /// <summary>
        /// The sort and beautify xml.
        /// </summary>
        /// <param name="xmlText">
        /// The xml text.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string XmlSortAndBeautify(string xmlText)
        {
            XElement root = XElement.Load(new XmlTextReader(new StringReader(xmlText)));

            XmlSort(root);

            string xml = root.ToString();

            XmlDocument document2 = new XmlDocument();

            document2.LoadXml(xml);

            var beautifulXml = document2.Beautify();

            return beautifulXml;
        }
    }
}