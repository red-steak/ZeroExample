using System.Xml.Schema;

namespace ViewModels.Helpers.Classes
{
    public class SumsGroupByModelSoldDuringEaster : IAgg
    {
        public string GroupByHeader => "Model";
        public string ConditionHeader => "Easter";

        public Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName()
        {
            return (items) => items
                .Where(x => IsEaster(x.SaleDate))
                .GroupBy(x => x.Model)
                .OrderBy(x => x.Key)
                .Select(x => new CarTotalsVM { Model = $"{x.Key}", Price = x.Sum(y => y?.Price), PriceWithVat = x.Sum(y => y?.PriceWithVat) });
        }

        static bool IsEaster(DateTime? date)
        {
            if (date is null)
                return false;

            var easterSunday = GetEasterSunday(date.Value.Year);

            var goodFriday = easterSunday.AddDays(-2);
            var easterMonday = easterSunday.AddDays(1);

            return date.Value.Date >= goodFriday.Date && date.Value.Date <= easterMonday.Date;
        }

        static DateTime GetEasterSunday(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateTime(year, month, day);
        }

    }
}