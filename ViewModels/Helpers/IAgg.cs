namespace ViewModels.Helpers
{
    public interface IAgg
    {
        string GroupByHeader { get; }
        string ConditionHeader { get; }
        Func<IEnumerable<CarVM>, IEnumerable<CarTotalsVM>> GetAggFunctionsByPropertyName();
    }
}
