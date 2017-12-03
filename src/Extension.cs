
namespace XmlCompareConsole
{
    using System.Text;
    using System.Xml;

    /// <summary>
    /// The extension.
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// The beautify method.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string Beautify(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
                                             {
                                                 Indent = true,
                                                 IndentChars = "  ",
                                                 NewLineChars = "\r\n",
                                                 NewLineHandling = NewLineHandling.Replace,
                                             };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }

            return sb.ToString();
        }
    }
}