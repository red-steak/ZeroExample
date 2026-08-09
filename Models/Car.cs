using System.Xml.Serialization;

namespace Models
{
    public class Car
    {
        public string? Model { get; set; }
        public double? Price { get; set; }
        public DateTime? SaleDate { get; set; }

        [XmlElement("VAT")]
        public double? Vat { get; set; }
    }
}
