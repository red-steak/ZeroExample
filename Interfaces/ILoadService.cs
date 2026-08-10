using System.Xml.Linq;

namespace Interfaces
{
    public interface ILoadService
    {
        Task<ParsingResult> LoadAsync(string xmlText);
    }
}
