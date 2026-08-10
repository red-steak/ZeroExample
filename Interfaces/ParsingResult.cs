using System.Xml.Linq;

namespace Interfaces
{
    public class ParsingResult(XDocument? document, bool result = true, string? errorMessage = null)
    {
        public XDocument? Document { get; } = document;
        public bool Result { get; set; } = result;
        public string? ErrorMessage { get; set; } = errorMessage;
    }
}
