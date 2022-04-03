using NuGet.Resources;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace NuGet.Authoring
{
    [XmlType("tag")]
    public class Tag
    {
        [Required(ErrorMessageResourceType = typeof(NuGetResources), ErrorMessageResourceName = "Manifest_TagIdRequired")]
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
