namespace BindingCleaner
{
    using System.Xml.Serialization;

    public class DependentAssembly
    {
        public DependentAssembly()
        {
            this.AssemblyIdentity = new AssemblyIdentity();
            this.BindingRedirect = new BindingRedirect();
        }

        [XmlElement("assemblyIdentity")]
        public AssemblyIdentity AssemblyIdentity { get; set; }

        [XmlElement("bindingRedirect")]
        public BindingRedirect BindingRedirect { get; set; }
    }
}