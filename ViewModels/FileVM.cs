using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels
{
    public partial class FileVM(string filePath, string fileName) : ReactiveObject
    {
        [Reactive] string _filePath = filePath;
        [Reactive] string _fileName = fileName;
    }
}