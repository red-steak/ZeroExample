using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels
{
    public partial class CarTotalsVM : ReactiveObject
    {
        [Reactive] string? _model;
        [Reactive] double? _price;
        [Reactive] double? _priceWithVat;
    }
}
