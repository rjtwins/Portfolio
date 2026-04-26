namespace BCT.Domain.Entities;
public class ProjectGridWizard : IdModel
{
    public ProjectGridWizard() { }

    public int ProjectId { get; set; }
    public double Interest { get; set; }
    public int Horizon { get; set; }
    public int StartYear { get; set; }
    public bool InterestEnabled { get; set; } = false;

    //Wizard options
    //Investement:
    public double NewInvestment { get; set; }
    public int NewInvestmentYear { get; set; } = 0;
    public string NewInvestmentDescription { get; set; } = string.Empty;

    public bool AvoidedInvestmentEnabled { get; set; } = false;
    public double AvoidedInvestment { get; set; }
    public int AvoidedInvestmentYear { get; set; }
    public string AvoidedInvestmentDescription { get; set; } = string.Empty;

    //Costs:
    //New Costs:
    public double ICTHardware { get; set; }
    public int ICTHardwareYear { get; set; }
    public string ICTHardwareDescription { get; set; } = string.Empty;

    public double ICTSoftware { get; set; }
    public int ICTSoftwareYear { get; set; }
    public string ICTSoftwareDescription { get; set; } = string.Empty;

    public double Equipment { get; set; }
    public int EquipmentYear { get; set; }
    public string EquipmentDescription { get; set; } = string.Empty;

    public double EquipmentUsage { get; set; }
    public int EquipmentUsageYear { get; set; }
    public string EquipmentUsageDescription { get; set; } = string.Empty;

    public double Personnel { get; set; }
    public int PersonnelYear { get; set; }
    public string PersonnelDescription { get; set; } = string.Empty;

    public double Energy { get; set; }
    public int EnergyYear { get; set; }
    public string EnergyDescription { get; set; } = string.Empty;

    public double Other { get; set; }
    public int OtherYear { get; set; }
    public string OtherDescription { get; set; } = string.Empty;

    //Avoided Costs:
    public bool AvoidedCostEnabled { get; set; } = false;

    public double EquipmentOvoided { get; set; }
    public int EquipmentOvoidedYear { get; set; }
    public string EquipmentOvoidedDescription { get; set; } = string.Empty;

    public double EquipmentUsageOvoided { get; set; }
    public string EquipmentUsageOvoidedDescription { get; set; } = string.Empty;
    public int EquipmentUsageOvoidedYear { get; set; }


    public double PersonnelOvoided { get; set; }
    public int PersonnelOvoidedYear { get; set; }
    public string PersonnelOvoidedDescription { get; set; } = string.Empty;

    public double EnergyOvoided { get; set; }
    public int EnergyOvoidedYear { get; set; }
    public string EnergyOvoidedDescription { get; set; } = string.Empty;

    public double OtherOvoided { get; set; }
    public int OtherOvoidedYear { get; set; }
    public string OtherOvoidedDescription { get; set; } = string.Empty;


    //Income:
    public double ExtraIncome { get; set; }
    //public int ExtraIncomeOption { get; set; } = 1;
    public int ExtraIncomeYear { get; set; }
    public string ExtraIncomeDescription { get; set; } = string.Empty;
    public bool ExtraIncomeEnabled { get; set; } = false;

    public double LostIncome { get; set; }
    //public int LostIncomeOption { get; set; } = 1;
    public int LostIncomeYear { get; set; }
    public string LostIncomeDescription { get; set; } = string.Empty;
    public bool LostIncomeEnabled { get; set; } = false;

    //Residual Value:
    public bool ResidualValueEnabled { get; set; } = false;
    public double ResidualValue { get; set; }
    //public int ResidualValueOption { get; set; } = 1;
    public string ResidualValueDescription { get; set; } = string.Empty;
}
