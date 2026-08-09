namespace ViewModels.Helpers.Classes
{
    public class SumsGroupByModel : IAgg
    {
        public string GroupByHeader => "Model";
        public string ConditionHeader => "Totals";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            return (items) => items
                .GroupBy(x => x.Model)
                .OrderBy(x => x.Key)
                .Select(x => new CarTotalsVM { Model = x.Key, Price = x.Sum(y => y?.Price), PriceWithVat = x.Sum(y => y?.PriceWithVat) });
        }
    }
}