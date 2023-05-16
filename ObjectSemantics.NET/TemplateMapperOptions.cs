namespace ObjectSemantics.NET
{
    public class TemplateMapperOptions
    {
        /// <summary>
        /// This will  apply XML Character Escape on invalid characters in a Property Value String with their valid XML Equivalent
        /// Example of Characters;
        /// " &quot;
        /// ' &apos;
        /// < &lt;
        /// > &gt;
        /// & &amp;
        /// </summary>
        public bool XmlCharEscaping { get; set; } = false;
    }
}
