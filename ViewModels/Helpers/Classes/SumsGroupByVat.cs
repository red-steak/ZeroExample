namespace ViewModels.Helpers.Classes
{
    public class SumsGroupByVat : IAgg
    {
        public string GroupByHeader => "Vat";
        public string ConditionHeader => "Totals";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            return (items) => items
                .GroupBy(x => x.Vat)
                .OrderBy(x => x.Key)
                .Select(x => new CarTotalsVM { Model = $"{x.Key} %", Price = x.Sum(y => y?.Price), PriceWithVat = x.Sum(y => y?.PriceWithVat) });
        }
    }
}