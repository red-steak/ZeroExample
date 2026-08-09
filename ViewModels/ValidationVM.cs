using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels
{
    public partial class ValidationVM : ReactiveObject
    {
        [Reactive] string _name = string.Empty;
        [Reactive] string _description = string.Empty;
        [Reactive] ValidationResult _result = ValidationResult.Undefined;
        [Reactive] string _resultDescrition = string.Empty;
    }
}
