using Radzen;

namespace BCT.Blazor.UIModels;

public class DataItem
{
    public int Year => (int)Math.Floor(X);
    public int Months => (int)((X - Math.Truncate(X)) * 12d) + 1;
    public DateTime DateTime => new DateTime(Year, Months == 0 ? 1 : Months, 2);
    public required double X { get; set; }
    public required string Key { get; set; }
    public required double Value { get; set; }
}

public record DashboardDataModel(
    string Payback,
    string PaybackDiff,
    BadgeStyle PayBackBadgeStyle,
    int ROI,
    BadgeStyle ROIBadgeStyle,
    int TotalResult,
    BadgeStyle TotalResultBadgeStyle,
    int TotalIncome,
    BadgeStyle TotalIncomeBadgeStyle,
    int TotalCost,
    BadgeStyle TotalCostBadgeStyle,
    int TotalInvestment,
    BadgeStyle TotalInvestmentBadgeStyle
);

public record AdditionalCriteriaItem(string Name, int Value, string Text);