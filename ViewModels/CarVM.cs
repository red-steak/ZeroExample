using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels
{
    public partial class CarVM : ReactiveObject
    {
        [Reactive] string? _model;
        [Reactive] DateTime? _saleDate;
        [Reactive] double? _price;
        [Reactive] double? _vat;
        [Reactive] double? _priceWithVat;
        
        #region ctor

        public CarVM()
        {
            this.WhenAnyValue(x => x.Price, x => x.Vat,
                    (price, vat) =>
                        price.HasValue && vat.HasValue
                            ? price.Value * (100 + vat.Value) / 100
                            : (double?)null)
                    .BindTo(this, x => x.PriceWithVat);
        }

        #endregion ctor
    }
}
