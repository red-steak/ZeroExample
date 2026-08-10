using Interfaces;
using System.Xml.Linq;

namespace Services
{
    public class LoadService : ILoadService
    {
        public async Task<ParsingResult> LoadAsync(string xmlText)
        {
            if (string.IsNullOrWhiteSpace(xmlText))
            {
                return new ParsingResult(null, result: false, errorMessage: "File content is empty.");
            }
            else if (!xmlText.StartsWith('<'))
            {
                return new ParsingResult(null, result: false, errorMessage: "File content does not appear to be valid XML.");
            }
            else if (!xmlText.EndsWith('>'))
            {
                return new ParsingResult(null, result: false, errorMessage: "File content does not appear to be valid XML.");
            }
            else if (!xmlText.Contains("</"))
            {
                return new ParsingResult(null, result: false, errorMessage: "File content does not appear to be valid XML.");
            }
            else if (!xmlText.Contains('<'))
            {
                return new ParsingResult(null, result: false, errorMessage: "File content does not appear to be valid XML.");
            }

            try
            {
                var document = await Task.FromResult(XDocument.Parse(xmlText));
                return new ParsingResult(document);
            }
            catch (Exception e)
            {
                return new ParsingResult(null, result: false, errorMessage: $"Failed to load XML content. Error: {e.Message}");
            }
        }

    }
}
