using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels
{
    public partial class FileVM(string filePath, string fileName, string fileContent) : ReactiveObject
    {
        [Reactive] string _filePath = filePath;
        [Reactive] string _fileName = fileName;
        [Reactive] string _defaultFileContent = fileContent;
    }
}