using System.Xml.Serialization;

namespace Test1.Entities
{
    [XmlInclude(typeof(Tourist))]
    [XmlInclude(typeof(Adult))]
    [XmlInclude(typeof(Child))]
    [XmlInclude(typeof(Infant))]
    public class TouristGroup : List<Tourist>
    {
    }
}
