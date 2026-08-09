namespace ViewModels.Helpers.Classes
{
    public class WeekendSumsGroupByModel : IAgg
    {
        public string GroupByHeader => "Model";
        public string ConditionHeader => "Weekend totals";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            static bool isWeekend(DateTime? date) => date?.DayOfWeek == DayOfWeek.Saturday || date?.DayOfWeek == DayOfWeek.Sunday;
            return (items) => items
                .Where(x => isWeekend(x.SaleDate))
                .GroupBy(x => x.Model)
                .OrderBy(x => x.Key)
                .Select(x => new CarTotalsVM { Model = x.Key, Price = x.Sum(y => y?.Price), PriceWithVat = x.Sum(y => y?.PriceWithVat) });
        }
    }
}