
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
                string contents = null;
                using (StreamReader s = new StreamReader(inputFile))
                {
                    var encoding = s.CurrentEncoding;
                    contents = s.ReadToEnd();
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
                }

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// The get comment block.
        /// </summary>
        /// <param name="allComments">
        /// The all comments.
        /// </param>
        /// <param name="currentComment">
        /// The current comment.
        /// </param>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<XComment> GetCommentBlock(
            IEnumerable<XComment> allComments,
            XComment currentComment,
            int frame)
        {
            frame++;
            var prevComment = allComments.Where(x => x.NextNode.NextNode == currentComment).ToList();

            if (prevComment.Count() == 0)
            {
                return new List<XComment>() { currentComment };
            }
            else
            {
                prevComment.InsertRange(0, GetCommentBlock(allComments, prevComment.First(), frame));
            }

            // prevComment.Add(currentComment);
            return prevComment;
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
        /// <param name="frame">
        /// The frame.
        /// </param>
        private static void XmlSort(XElement element, int frame)
        {
            frame++;
            if (element.Elements().Count() <= 1)
            {
                return;
            }

            var comments = element.Nodes().OfType<XComment>().ToList();

            var orderedElements = element.Elements().OrderBy(
                xElement => xElement.Name + SortAndAggregateAttributesAsString(xElement.Attributes())).ToList();

            foreach (XElement xElement in orderedElements)
            {
                XmlSort(xElement, frame);
            }

            Dictionary<XElement, List<XComment>> elementComments = new Dictionary<XElement, List<XComment>>();

            foreach (XElement xElement in element.Elements())
            {
                var commentsBeforeXElement = comments.FirstOrDefault(
                    x =>
                        {
                            if (x.NextNode.NextNode == null)
                            {
                                return false;
                            }

                            var hashcode1 = x.NextNode.NextNode.GetHashCode();

                            var hashcode2 = xElement.GetHashCode();

                            return hashcode1 == hashcode2;
                        });

                if (commentsBeforeXElement != null)
                {
                    var commentBlock = GetCommentBlock(comments, commentsBeforeXElement, 0);

                    commentBlock.ForEach(element.Add);

                    elementComments.Add(xElement, commentBlock);
                }
            }

            var commentsAtEnd = comments.FirstOrDefault(
                x =>
                    {
                        var elements = x.ElementsAfterSelf();
                        return elements.Count() == 0;
                    });

            List<XComment> commentBlockAtEnd = null;

            if (commentsAtEnd != null)
            {
                commentBlockAtEnd = GetCommentBlock(comments, commentsAtEnd, 0);
            }

            element.RemoveNodes();

            foreach (XElement xElement in orderedElements)
            {
                if (elementComments.ContainsKey(xElement))
                {
                    elementComments[xElement].ForEach(element.Add);
                }

                element.Add(xElement);
            }

            commentBlockAtEnd?.ForEach(element.Add);
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

            XmlSort(root, 0);

            string xml = root.ToString();

            XmlDocument document2 = new XmlDocument();

            document2.LoadXml(xml);

            var beautifulXml = document2.Beautify();

            return beautifulXml;
        }
    }
}