namespace BCT.Domain.Entities;
public class Scenario : IdModel
{
    public int ProjectId { get; set; }
    public double InvestmentMod { get; set; }
    public double AvoidedInvestmentMod { get; set; }
    public double CostMod { get; set; }
    public double AvoidedCostMod { get; set; }
    public double IncomeMod { get; set; }
    public double LostIncomeMod { get; set; }

    public double InvestmentDif { get; set; }
    public double AvoidedInvestmentDif { get; set; }
    public double CostDif { get; set; }
    public double AvoidedCostDif { get; set; }
    public double IncomeDif { get; set; }
    public double LostIncomeDif { get; set; }

    public Enums.ScnearioMode Mode { get; set; } = Enums.ScnearioMode.Relative;
}
