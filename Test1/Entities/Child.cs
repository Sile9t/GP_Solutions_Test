using System.Xml.Serialization;

namespace Test1.Entities
{
    public record Child : Tourist
    {
        [XmlAttribute]
        public DateTime BirthDate { get; set; }
    }
}
