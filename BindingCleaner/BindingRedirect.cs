namespace BindingCleaner
{
    using System.Xml.Serialization;

    public class BindingRedirect
    {
        [XmlAttribute("oldVersion")]
        public string OldVersion { get; set; }

        [XmlAttribute("newVersion")]
        public string NewVersion { get; set; }
    }
}