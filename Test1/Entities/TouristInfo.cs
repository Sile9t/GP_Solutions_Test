using System.Xml.Serialization;

namespace Test1.Entities
{
    [XmlRoot(IsNullable = false)]
    public record TouristInfo
    {
        [XmlArrayItem(typeof(TouristGroup))]
        public List<TouristGroup> TouristGroups { get; set; } = new List<TouristGroup>();
    }
}
