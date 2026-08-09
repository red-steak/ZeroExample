namespace ViewModels.Helpers.Classes
{
    public class SumsGroupByDayOfWeek : IAgg
    {
        public string GroupByHeader => "Day of week";
        public string ConditionHeader => "Totals";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            return (items) => items
                .GroupBy(x => x.SaleDate?.DayOfWeek)
                .OrderBy(x => x.Key == 0 ? 7 : (int?)x.Key)
                .Select(x => new CarTotalsVM
                {
                    Model = $"{x.Max(d => d.SaleDate)?.ToString("dddd")}",
                    Price = x.Sum(y => y?.Price),
                    PriceWithVat = x.Sum(y => y?.PriceWithVat)
                });
        }
    }
}