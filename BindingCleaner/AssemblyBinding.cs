namespace BindingCleaner
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(Namespace = "urn:schemas-microsoft-com:asm.v1", ElementName = "assemblyBinding")]
    [XmlType("assemblyBinding")]
    public class AssemblyBinding
    {
        [XmlElement("dependentAssembly")]
        public List<DependentAssembly> DependentAssemblies { get; set; }
    }
}