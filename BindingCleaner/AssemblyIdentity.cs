namespace BindingCleaner
{
    using System.Xml.Serialization;

    public class AssemblyIdentity
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("publicKeyToken")]
        public string PublicKeyToken { get; set; }

        [XmlAttribute("culture")]
        public string Culture { get; set; }
    }
}