namespace ViewModels.Helpers.Classes
{
    public class NonWeekendSumsGroupByModel : IAgg
    {
        public string GroupByHeader => "Model";
        public string ConditionHeader => "NON weekend totals";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            static bool isNotWeekend(DateTime? date) => date?.DayOfWeek != DayOfWeek.Saturday && date?.DayOfWeek != DayOfWeek.Sunday;
            return (items) => items
                .Where(x => isNotWeekend(x.SaleDate))
                .GroupBy(x => x.Model)
                .OrderBy(x => x.Key)
                .Select(x => new CarTotalsVM { Model = x.Key, Price = x.Sum(y => y?.Price), PriceWithVat = x.Sum(y => y?.PriceWithVat) });
        }
    }
}