using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels.Helpers
{
    public partial class AggHelper<T> : ReactiveObject where T : class, IAgg, new() 
    {
        [Reactive] IEnumerable<CarTotalsVM>? _totals;

        public AggHelper<T> Calculate(List<CarVM> values)
        {
            var aggFunctions = new T().GetAggFunctionsByPropertyName();
            Totals = aggFunctions.Invoke(values);
            return this;
        }

        public static string GetGroupByHeader()
        {
            return new T().GroupByHeader;
        }

        public static string GetConditionHeader()
        {
            return new T().ConditionHeader;
        }

        public string GroupByHeader { get; } = GetGroupByHeader();
        public string ConditionHeader { get; } = GetConditionHeader();

        [Reactive] string _header = $"{GetConditionHeader()} group by {GetGroupByHeader().ToLowerInvariant()}";
    }
}
