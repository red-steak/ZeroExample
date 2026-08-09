namespace ViewModels.Helpers.Classes
{
    public class SumsGroupByYear : IAgg
    {
        public string GroupByHeader => "Year";
        public string ConditionHeader => "Totals";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            return (items) => items
                .GroupBy(x => x.SaleDate?.Year)
                .OrderBy(x => x.Key)
                .Select(x => new CarTotalsVM { Model = $"{x.Key}", Price = x.Sum(y => y?.Price), PriceWithVat = x.Sum(y => y?.PriceWithVat) });
        }
    }
}